using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class GuidIdGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var guidIds = cyrusGenerationContext.GuidIds;
        foreach (var recordSymbol in guidIds)
        {
            var sourceText = GenerateGuidId(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol.Name}.g.cs", sourceText);
        };

        if (guidIds.Any())
        {
            var guidModels = GetPartialModelClass(
                guidIds.First().ContainingAssembly.Name,
                "TypedIds",
                "Guids",
                "string",
                guidIds.Select(guid => $"\"{guid.Name}\""),
                cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-guids.g.cs", guidModels);
        }
    }

    private string GenerateGuidId(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty(),
            UnderlyingType = item.GetUnderlyingTypeOfTypedId(),
            Default = "Guid.Empty"
        };

        var source = liquidEngine.Render(model, "GuidId");

        return source;
    }
}