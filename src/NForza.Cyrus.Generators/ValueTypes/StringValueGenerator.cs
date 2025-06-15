using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes;

public class StringValueGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var StringValues = cyrusGenerationContext.StringValues;
        foreach (var recordSymbol in StringValues)
        {
            var sourceText = GenerateCodeForRecordStruct(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol.Name}.g.cs", sourceText);
        };
        if (StringValues.Any())
        {
            var stringModels = GetPartialModelClass(StringValues.First().ContainingAssembly.Name, "TypedIds", "Strings", "string", StringValues.Select(s => $"\"{s.Name}\""), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-strings.g.cs", stringModels);
        }
    }

    private string GenerateCodeForRecordStruct(INamedTypeSymbol item, LiquidEngine liquidEngine)
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
            HasMaximumAndMinumum = min.HasValue && max.HasValue
        };

        var resolvedSource = liquidEngine.Render(model, "StringValue");

        return resolvedSource;
    }

    private (int? min, int? max) GetMinAndMaxFromType(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "StringValueAttribute");
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