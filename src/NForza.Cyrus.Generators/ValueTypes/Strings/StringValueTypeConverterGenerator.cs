using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes.Strings;

public class StringValueTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var StringValues = cyrusGenerationContext.StringValues;
        foreach (var recordSymbol in StringValues)
        {
            var sourceText = GenerateStringValueTypeConverter(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", sourceText);
        }
        ;
    }

    private string GenerateStringValueTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty()
        };
        var resolvedSource = liquidEngine.Render(model, "StringValueTypeConverter");
        return resolvedSource;
    }
}