using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class HttpContextQueryFactoryGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var assemblyReferences = context.CompilationProvider;

        var configurationProvider = ParseConfigFile<CyrusConfig>(context, "cyrusConfig.yaml");

        var typesFromReferencedAssembly = assemblyReferences
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.References
                    .Select(ca => compilation.GetAssemblyOrModuleSymbol(ca) as IAssemblySymbol)
                    .SelectMany(ass => ass != null ? GetAllTypesRecursively(ass.GlobalNamespace) : [])
                    .Where(IsQuery)
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
            if (config.GenerationType.Contains("webapi") && queryHandlers.Any())
            {
                var sourceText = GenerateQueryFactoryExtensionMethods(queryHandlers);
                spc.AddSource($"HttpContextQueryFactory.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private bool IsQuery(INamedTypeSymbol symbol)
    {
        Debug.WriteLine(symbol.Name);
        bool isStruct = symbol.TypeKind == TypeKind.Struct;
        bool hasQueryName = symbol.Name.EndsWith("Query");
        return isStruct && hasQueryName;
    }

    private static string[] assembliesToSkip = new[] { "System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle" };
    private IEnumerable<INamedTypeSymbol> GetAllTypesRecursively(INamespaceSymbol ns)
    {
        var assemblyName = ns?.ContainingAssembly?.Name;
        if (assemblyName != null && assembliesToSkip.Any( n => assemblyName.StartsWith(n)))
        {
            return [];
        }

        var types = ns.GetTypeMembers();
        foreach (var subNamespace in ns.GetNamespaceMembers())
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
                .OfType<IPropertySymbol>() // Select only properties
                .Where(p => p.DeclaredAccessibility == Accessibility.Public); // Filter by accessibility
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
