using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class GuidIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var recordSymbols = cyrusGenerationContext.GuidIds;
        foreach (var recordSymbol in recordSymbols)
        {
            var sourceText = GenerateGuidIdTypeConverter(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", sourceText);
        };
    }

    private string GenerateGuidIdTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            TypedIdName = item.Name,
            NamespaceName = item.ContainingNamespace.GetNameOrEmpty()
        };

        string source = liquidEngine.Render(model, "GuidIdTypeConverter");

        return source;
    }
}