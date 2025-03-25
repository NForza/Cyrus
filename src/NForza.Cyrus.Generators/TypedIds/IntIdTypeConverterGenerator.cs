using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.TypedIds;

public class IntIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var recordSymbols = cyrusProvider.IntIds;
        foreach (var recordSymbol in recordSymbols)
        {
            var sourceText = GenerateGuidIdTypeConverter(recordSymbol);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateGuidIdTypeConverter(INamedTypeSymbol item)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.ToDisplayString()
        };

        string source = LiquidEngine.Render(model, "IntIdTypeConverter");

        return source;
    }
}