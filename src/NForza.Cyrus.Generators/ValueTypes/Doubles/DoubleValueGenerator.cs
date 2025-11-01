using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes.Doubles;

public class DoubleValueGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var recordSymbols = cyrusGenerationContext.DoubleValues;
        foreach (var recordSymbol in recordSymbols)
        {
            var sourceText = GenerateDoubleValueTypes(recordSymbol, cyrusGenerationContext.LiquidEngine);
            context.AddSource($"{recordSymbol.Name}.g.cs", sourceText);
        }
    }

    private string GenerateDoubleValueTypes(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        (int? min, int? max) = GetMinAndMaxFromType(item);
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty(),
            UnderlyingType = item.GetUnderlyingTypeOfValueType(),
            Minimum = min,
            HasMinimum = min.HasValue,
            Maximum = max,
            HasMaximum = max.HasValue,
            HasMaximumAndMinumum = min.HasValue && max.HasValue,
            HasMaximumOrMinumum = min.HasValue || max.HasValue
        };
        var source = liquidEngine.Render(model, "DoubleValue");

        return source;
    }

    private (int? min, int? max) GetMinAndMaxFromType(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "DoubleValueAttribute");
        if (attribute == null)
        {
            return (null, null);
        }
        var args = attribute.ConstructorArguments.Select(a => int.Parse(a.Value?.ToString())).ToList();
        int? min = args.Count > 0 ? args[0] : null;
        int? max = args.Count > 1 ? args[1] : null;
        return (min, max);
    }
}