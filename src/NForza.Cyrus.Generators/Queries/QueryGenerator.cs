using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Model;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Queries;

public class QueryGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var queryHandlers = cyrusProvider.QueryHandlers;
        if (queryHandlers.Any())
        {
            string assemblyName = queryHandlers.First().ContainingAssembly.Name;
            var eventModels = GetPartialModelClass(
                assemblyName,
                "Queries",
                "Queries",
                "ModelQueryDefinition",
                queryHandlers.Select(e => ModelGenerator.ForQueryHandler(e, LiquidEngine)));
            spc.AddSource($"model-queries.g.cs", SourceText.From(eventModels, Encoding.UTF8));

            var referencedTypes = queryHandlers.Select(q => q.GetQueryReturnType()).SelectMany(cs => cs.GetReferencedTypes());
            var referencedTypeModels = GetPartialModelClass(assemblyName, "Queries", "Models", "ModelQueryDefinition", referencedTypes.Select(cm => ModelGenerator.ForNamedType(cm, LiquidEngine)));
        }
    }
}