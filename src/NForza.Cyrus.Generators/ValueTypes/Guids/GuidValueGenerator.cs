using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes.Guids;

public class GuidValueGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var guidValues = cyrusGenerationContext.GuidValues;
        foreach (var recordSymbol in guidValues)
        {
            var sourceText = GenerateGuidValue(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol.Name}.g.cs", sourceText);
        }       
    }

    private string GenerateGuidValue(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty(),
            UnderlyingType = item.GetUnderlyingTypeOfValueType(),
            Default = "Guid.Empty"
        };

        var source = liquidEngine.Render(model, "GuidValue");

        return source;
    }
}