using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Commands;

public class ApiEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        if (cyrusProvider.GenerationConfig != null && cyrusProvider.GenerationConfig.GenerationTarget.Contains(GenerationTarget.WebApi) && (cyrusProvider.AllHandlers.Any()))
        {
            var allCommandHandlers = cyrusProvider.AllCommandHandlers;
            var allQueryHandlers = cyrusProvider.AllQueryHandlers;
            string assemblyName = cyrusProvider.AllHandlers.First().ContainingAssembly.Name;

            IEnumerable<(INamedTypeSymbol NamedTypeSymbol, string httpVerb, string? Route)> commandEndpointSymbols = allCommandHandlers
                .Select(nts => ((INamedTypeSymbol)nts.Parameters[0].Type, nts.GetCommandVerb(), nts.GetCommandRoute()));

            IEnumerable<(INamedTypeSymbol NamedTypeSymbol, string httpVerb, string? Route)> queryEndpointSymbols = allQueryHandlers
                .Select(nts => ((INamedTypeSymbol)nts.Parameters[0].Type, "Get", nts.GetQueryRoute()));

            IEnumerable<string> commandEndpoints = commandEndpointSymbols
                  .Where(x => !string.IsNullOrEmpty(x.Route))
                  .Select(em => ModelGenerator.ForCommandEndpoint(em, cyrusProvider.LiquidEngine));

            IEnumerable<string> queryEndpoints = queryEndpointSymbols
                  .Where(x => !string.IsNullOrEmpty(x.Route))
                  .Select(em => ModelGenerator.ForQueryEndpoint(em, cyrusProvider.LiquidEngine));

            var endpointModels = GetPartialModelClass(assemblyName, "Endpoints", "Endpoints", "ModelEndpointDefinition",
                commandEndpoints.Concat(queryEndpoints), cyrusProvider.LiquidEngine);

            spc.AddSource($"model-endpoints.g.cs", SourceText.From(endpointModels, Encoding.UTF8));
        }
    }
}