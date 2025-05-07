using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class IntIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        var recordSymbols = cyrusProvider.IntIds;
        foreach (var recordSymbol in recordSymbols)
        {
            var sourceText = GenerateGuidIdTypeConverter(recordSymbol, cyrusProvider.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateGuidIdTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.ToDisplayString()
        };

        string source = liquidEngine.Render(model, "IntIdTypeConverter");

        return source;
    }
}