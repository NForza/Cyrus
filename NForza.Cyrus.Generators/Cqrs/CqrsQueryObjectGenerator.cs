using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cyrus.Cqrs.Generator;

[Generator]
public class CqrsQueryFactoryGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var assemblyReferences = context.CompilationProvider;

        var configurationProvider = ParseConfigFile<CqrsConfig>(context, "cyrusConfig.yaml");

        var typesFromReferencedAssembly = assemblyReferences
            .SelectMany((compilation, _) =>
            {
                var typesFromContractAssemblies = compilation.References
                    .Select(ca => compilation.GetAssemblyOrModuleSymbol(ca) as IAssemblySymbol)
                    .Where(ass => ass != null && ass.Name.EndsWith("Contracts"))
                    .SelectMany(ass => GetAllTypesRecursively(ass.GlobalNamespace))
                    .Where(IsQuery)
                    .ToList();
                return typesFromContractAssemblies;
            })
            .Where(IsQuery)
            .Collect();

        var combinedProvider = typesFromReferencedAssembly.Combine(configurationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, queryHandlersWithConfig) =>
        {
            var (queryHandlers, config) = queryHandlersWithConfig;
            if (config?.GenerationType == "webapi")
            {
                var sourceText = GenerateQueryFactoryExtensionMethods(queryHandlers);
                spc.AddSource($"HttpContextQueryFactory.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private bool IsQuery(INamedTypeSymbol symbol) 
        => symbol.Name.EndsWith("Query") && symbol.TypeKind == TypeKind.Struct;

    private IEnumerable<INamedTypeSymbol> GetAllTypesRecursively(INamespaceSymbol ns)
    {
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
            foreach(var prop in GetPublicProperties(query))
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
