using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class StringIdTypeConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        var stringIds = cyrusProvider.StringIds;
        foreach (var recordSymbol in stringIds)
        {
            var sourceText = GenerateStringIdTypeConverter(recordSymbol, cyrusProvider.LiquidEngine);
            spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateStringIdTypeConverter(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var model = new
        {
            item.Name,
            Namespace = fullyQualifiedNamespace
        };
        var resolvedSource = liquidEngine.Render(model, "StringIdTypeConverter");
        return resolvedSource;
    }
}