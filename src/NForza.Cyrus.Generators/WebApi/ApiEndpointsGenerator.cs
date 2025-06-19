using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

public class ApiEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        if (cyrusGenerationContext.GenerationConfig != null && cyrusGenerationContext.GenerationConfig.GenerationTarget.Contains(GenerationTarget.WebApi) && cyrusGenerationContext.AllHandlers.Any())
        {
            var allCommandHandlers = cyrusGenerationContext.AllCommandHandlers;
            var allQueryHandlers = cyrusGenerationContext.AllQueryHandlers;
            string assemblyName = cyrusGenerationContext.AllHandlers.First().ContainingAssembly.Name;

            IEnumerable<(INamedTypeSymbol NamedTypeSymbol, string httpVerb, string? Route)> commandEndpointSymbols = allCommandHandlers
                .Select(nts => ((INamedTypeSymbol)nts.Parameters[0].Type, nts.GetCommandVerb(), nts.GetCommandRoute()));

            IEnumerable<(INamedTypeSymbol NamedTypeSymbol, string httpVerb, string? Route)> queryEndpointSymbols = allQueryHandlers
                .Select(nts => ((INamedTypeSymbol)nts.Parameters[0].Type, "Get", nts.GetQueryRoute()));

            IEnumerable<string> commandEndpoints = commandEndpointSymbols
                  .Where(x => !string.IsNullOrEmpty(x.Route))
                  .Select(em => ModelGenerator.ForCommandEndpoint(em, cyrusGenerationContext.LiquidEngine));

            IEnumerable<string> queryEndpoints = queryEndpointSymbols
                  .Where(x => !string.IsNullOrEmpty(x.Route))
                  .Select(em => ModelGenerator.ForQueryEndpoint(em, cyrusGenerationContext.LiquidEngine));

            var endpointModels = GetPartialModelClass(assemblyName, "Endpoints", "Endpoints", "ModelEndpointDefinition",
                commandEndpoints.Concat(queryEndpoints), cyrusGenerationContext.LiquidEngine);

            spc.AddSource($"model-endpoints.g.cs", endpointModels);
        }
    }
}