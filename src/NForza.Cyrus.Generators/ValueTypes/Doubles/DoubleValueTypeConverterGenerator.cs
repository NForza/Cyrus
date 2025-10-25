using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes.Doubles;

public class DoubleValueTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var recordSymbols = cyrusGenerationContext.DoubleValues;
        foreach (var recordSymbol in recordSymbols)
        {
            var sourceText = GenerateGuidValueTypeConverter(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", sourceText);
        }
        ;
    }

    private string GenerateGuidValueTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty()
        };

        string source = liquidEngine.Render(model, "DoubleValueTypeConverter");

        return source;
    }
}