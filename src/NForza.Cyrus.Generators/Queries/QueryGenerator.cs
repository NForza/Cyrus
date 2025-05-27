using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Queries;

public class QueryGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var queryHandlers = cyrusGenerationContext.QueryHandlers;
        if (queryHandlers.Any())
        {
            string assemblyName = queryHandlers.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Queries",
                "Queries",
                "ModelQueryDefinition",
                queryHandlers.Select(e => ModelGenerator.ForQueryHandler(e, cyrusGenerationContext.LiquidEngine)),
                cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-queries.g.cs", SourceText.From(eventModels, Encoding.UTF8));

            var referencedTypes = queryHandlers.Select(q => q.GetQueryReturnType()).SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Queries", "Models", "ModelQueryDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, cyrusGenerationContext.LiquidEngine)), cyrusGenerationContext.LiquidEngine);
        }
    }
}