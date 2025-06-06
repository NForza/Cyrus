using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class StringIdGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var stringIds = cyrusGenerationContext.StringIds;
        foreach (var recordSymbol in stringIds)
        {
            var sourceText = GenerateCodeForRecordStruct(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
        if (stringIds.Any())
        {
            var stringModels = GetPartialModelClass(stringIds.First().ContainingAssembly.Name, "TypedIds", "Strings", "string", stringIds.Select(s => $"\"{s.Name}\""), cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"model-strings.g.cs", SourceText.From(stringModels, Encoding.UTF8));
        }
    }

    private string GenerateCodeForRecordStruct(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        (int? min, int? max) = GetMinAndMaxFromType(item);
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty(),
            UnderlyingType = item.GetUnderlyingTypeOfTypedId(),
            Minimum = min,
            HasMinimum = min.HasValue,
            Maximum = max,
            HasMaximum = max.HasValue,
            HasMaximumAndMinumum = min.HasValue && max.HasValue
        };

        var resolvedSource = liquidEngine.Render(model, "StringId");

        return resolvedSource;
    }

    private (int? min, int? max) GetMinAndMaxFromType(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "StringIdAttribute");
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