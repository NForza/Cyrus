using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class HttpContextCqrsFactoryGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var assemblyReferences = context.CompilationProvider;

        var configurationProvider = ConfigProvider(context);

        var typesFromReferencedAssembly = assemblyReferences
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.References
                    .Select(ca => compilation.GetAssemblyOrModuleSymbol(ca) as IAssemblySymbol)
                    .SelectMany(ass => ass != null ? GetAllTypesRecursively(ass.GlobalNamespace) : [])
                    .Where(t => IsQuery(t) || IsCommand(t))
                    .ToList();

                var csharpCompilation = (CSharpCompilation)compilation;
                var typesInCompilation = GetAllTypesRecursively(csharpCompilation.GlobalNamespace)
                    .Where(IsQuery)
                    .ToList();

                return typesFromAssemblies.Union(typesInCompilation, SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>();
            })
           .Collect();

        var combinedProvider = typesFromReferencedAssembly.Combine(configurationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, queryHandlersWithConfig) =>
        {
            var (queryHandlers, config) = queryHandlersWithConfig;
            if (config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                var sourceText = GenerateQueryFactoryExtensionMethods(queryHandlers);
                spc.AddSource($"HttpContextObjectFactory.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private bool IsQuery(INamedTypeSymbol symbol)
    {
        Debug.WriteLine(symbol.Name);
        bool hasQueryName = symbol.Name.EndsWith("Query");
        bool isFrameworkAssembly = symbol.ContainingAssembly.IsFrameworkAssembly();
        return hasQueryName && !isFrameworkAssembly;
    }

    private bool IsCommand(INamedTypeSymbol symbol)
    {
        Debug.WriteLine(symbol.Name);
        bool hasCommandName = symbol.Name.EndsWith("Command");
        bool isFrameworkAssembly = symbol.ContainingAssembly.IsFrameworkAssembly();
        return hasCommandName && !isFrameworkAssembly;
    }

    private static string[] assembliesToSkip = ["System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle", "RabbitMQ", "MassTransit"];
    private IEnumerable<INamedTypeSymbol> GetAllTypesRecursively(INamespaceSymbol namespaceSymbol)
    {
        var assemblyName = namespaceSymbol?.ContainingAssembly?.Name;
        if (assemblyName != null && assembliesToSkip.Any(n => assemblyName.StartsWith(n)))
        {
            return [];
        }

        var types = namespaceSymbol.GetTypeMembers();
        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            types = types.AddRange(GetAllTypesRecursively(subNamespace));
        }
        return types;
    }

    private string GenerateQueryFactoryExtensionMethods(ImmutableArray<INamedTypeSymbol> queries)
    {
        StringBuilder source = new();
        foreach (var query in queries)
        {
            var queryTypeName = query.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            source.Append($@"    objectFactories.Add(typeof({queryTypeName}), (ctx) => {GetConstructionExpression(query)}");
        }
        
        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("HttpContextCqrsFactory.cs", new Dictionary<string, string>
        {
            ["QueryFactoryMethod"] = source.ToString()
        });

        return resolvedSource;
    }

    private string GetConstructionExpression(INamedTypeSymbol query)
    {
        static IEnumerable<IPropertySymbol> GetPublicProperties(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        }

        var queryTypeName = query.ToFullName();
        var ctor = new StringBuilder(@$"new {queryTypeName}");
        var constructorProperties = GenerateConstructorParameters(query, ctor);
        var propertiesToInitialize = GetPublicProperties(query).Where(p => !constructorProperties.Contains(p.Name)).ToList();
        if (propertiesToInitialize.Count > 0)
        {
            ctor.Append("{");
            var propertyInitializer = new List<string>();
            foreach (var prop in propertiesToInitialize)
            {
                propertyInitializer.Add(@$"{prop.Name} = ({prop.Type.ToFullName()})GetPropertyValue(""{prop.Name}"", ctx, typeof({prop.Type}))");
            }
            ctor.Append(string.Join(", ", propertyInitializer));
            ctor.Append("}");
        }
        ctor.AppendLine(");");
        return ctor.ToString();
    }

    private List<string> GenerateConstructorParameters(INamedTypeSymbol query, StringBuilder ctor)
    {
        var constructorWithLeastParameters = query.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public) 
                .OrderBy(c => c.Parameters.Length)
                .FirstOrDefault();
        if (constructorWithLeastParameters == null)
        {
            return [];
        }
        ctor.Append("(");

        var result = new List<string>();
        var firstParam = constructorWithLeastParameters.Parameters.FirstOrDefault();
        foreach (var param in constructorWithLeastParameters.Parameters)
        {
            if (!param.Equals(firstParam, SymbolEqualityComparer.Default))
            {
                ctor.Append(", ");
            }
            ctor.Append($"({param.Type.ToFullName()})GetPropertyValue(\"{param.Name}\", ctx, typeof({param.Type.ToFullName()}))");
            result.Add(param.Name);
        }
        ctor.Append(")");
        return result;
    }
}
