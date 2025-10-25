using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes.Guids;

public class GuidValueTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var recordSymbols = cyrusGenerationContext.GuidValues;
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
            TypedIdName = item.Name,
            NamespaceName = item.ContainingNamespace.GetNameOrEmpty()
        };

        string source = liquidEngine.Render(model, "GuidValueTypeConverter");

        return source;
    }
}