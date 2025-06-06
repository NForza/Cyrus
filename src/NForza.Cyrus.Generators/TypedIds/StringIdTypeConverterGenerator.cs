using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class StringIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var stringIds = cyrusGenerationContext.StringIds;
        foreach (var recordSymbol in stringIds)
        {
            var sourceText = GenerateStringIdTypeConverter(recordSymbol, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateStringIdTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.GetNameOrEmpty()
        };
        var resolvedSource = liquidEngine.Render(model, "StringIdTypeConverter");
        return resolvedSource;
    }
}