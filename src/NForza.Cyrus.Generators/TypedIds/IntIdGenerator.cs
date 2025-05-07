using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class IntIdGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusProvider)
    {
        var recordSymbols = cyrusProvider.IntIds;
            foreach (var recordSymbol in recordSymbols)
            {
                var sourceText = GenerateIntId(recordSymbol, cyrusProvider.LiquidEngine);
                context.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };

            if (recordSymbols.Any())
            {
                var intModels = GetPartialModelClass(recordSymbols.First().ContainingAssembly.Name, "TypedIds", "Integers", "string", recordSymbols.Select(im => $"\"{im.Name}\""), cyrusProvider.LiquidEngine);
                context.AddSource($"model-ints.g.cs", SourceText.From(intModels, Encoding.UTF8));
            }
    }

    private string GenerateIntId(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        (int? min, int? max) = GetMinAndMaxFromType(item);
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.ToDisplayString(),
            UnderlyingType = item.GetUnderlyingTypeOfTypedId(),
            Minimum = min,
            HasMinimum = min.HasValue,
            Maximum = max,
            HasMaximum = max.HasValue,
            HasMaximumAndMinumum = min.HasValue && max.HasValue,
            HasMaximumOrMinumum = min.HasValue || max.HasValue
        };
        var source = liquidEngine.Render(model, "IntId");

        return source;
    }

    private (int? min, int? max) GetMinAndMaxFromType(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "IntIdAttribute");
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