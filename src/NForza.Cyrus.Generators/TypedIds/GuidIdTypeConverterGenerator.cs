using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class GuidIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var recordSymbols = cyrusProvider.GuidIds;
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
            TypedIdName = item.Name,
            NamespaceName = item.ContainingNamespace.ToDisplayString()
        };

        string source = LiquidEngine.Render(model, "GuidIdTypeConverter");

        return source;
    }
}