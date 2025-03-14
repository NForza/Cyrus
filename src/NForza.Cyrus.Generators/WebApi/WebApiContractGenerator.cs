using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class WebApiContractGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var compilationProvider = context.CompilationProvider;

        var typesFromReferencedAssemblyProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies();

                var contractsFromAssemblies = typesFromAssemblies
                    .Where(t => t.IsCommand() || t.IsQuery());
                
                return contractsFromAssemblies;
            })
           .Collect();

        var serviceCollectionCombinedProvider = context
            .CompilationProvider
            .Combine(typesFromReferencedAssemblyProvider)
            .Combine(configProvider);

        context.RegisterSourceOutput(serviceCollectionCombinedProvider, (sourceProductionContext, source) =>
        {
            var ((compilation, typesFromReferencedAssemblies), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                foreach (var contract in typesFromReferencedAssemblies)
                {
                    IEnumerable<IPropertySymbol> properties = contract.GetPublicProperties();
                    var model = new 
                    { 
                        Namespace = contract.ContainingNamespace, 
                        Name = contract.Name,
                        FullName = contract.ToFullName(),
                        Properties = properties.Select(p => new { Name = p.Name, Type = p.Type.ToFullName(), IsNullable = p.Type.IsNullable() }).ToList()
                    };

                    var fileContents = LiquidEngine.Render(model, "WebApiContract");

                    sourceProductionContext.AddSource(
                       $"{contract.Name}Contract.g.cs",
                       SourceText.From(fileContents, Encoding.UTF8));
                }
            }
        });
    }
}