using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class GuidIdGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var guidIds = cyrusProvider.GuidIds;
        foreach (var recordSymbol in guidIds)
        {
            var sourceText = GenerateGuidId(recordSymbol);
            spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };

        if (guidIds.Any())
        {
            var guidModels = GetPartialModelClass(
                guidIds.First().ContainingAssembly.Name,
                "TypedIds",
                "Guids",
                "string",
                guidIds.Select(guid => $"\"{guid.Name}\""));
            spc.AddSource($"model-guids.g.cs", SourceText.From(guidModels, Encoding.UTF8));
        }
    }

    private string GenerateGuidId(INamedTypeSymbol item)
    {
        var model = new
        {
            item.Name,
            Namespace = item.ContainingNamespace.ToDisplayString(),
            UnderlyingType = item.GetUnderlyingTypeOfTypedId(),
            Default = "Guid.Empty"
        };

        var source = LiquidEngine.Render(model, "GuidId");

        return source;
    }
}