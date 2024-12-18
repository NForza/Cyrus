using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.Generators.Config;
using NForza.Generators;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class HttpContextCqrsFactoryGenerator : GeneratorBase, IIncrementalGenerator
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

    private static string[] assembliesToSkip = new[] { "System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle" };
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
        static IEnumerable<IPropertySymbol> GetPublicProperties(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        }

        StringBuilder source = new();
        foreach (var query in queries)
        {
            source.Append($@"    objectFactories.Add(typeof({query}), (ctx) => new {query}{{");

            var propertyInitializer = new List<string>();
            foreach (var prop in GetPublicProperties(query))
            {
                propertyInitializer.Add(@$"{prop.Name} = ({prop.Type})GetPropertyValue(""{prop.Name}"", ctx, typeof({prop.Type}))");
            }
            source.Append(string.Join(", ", propertyInitializer));
            source.AppendLine($@"}});");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("HttpContextQueryFactory.cs", new Dictionary<string, string>
        {
            ["QueryFactoryMethod"] = source.ToString()
        });

        return resolvedSource;
    }
}
