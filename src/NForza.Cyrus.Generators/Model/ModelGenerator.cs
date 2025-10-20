using System.Linq;
using System.Text.Json;
using Cyrus;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Model;

public class ModelGenerator : CyrusGeneratorBase
{
    public static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = false };

    public override void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var commands = cyrusGenerationContext.Commands.Select(c => new { c.Name, Properties = c.GetPublicProperties().Select(p => new { p.Name }) });

        var model = new
        {
            Commands = commands
        };
        var modelJson = JsonSerializer.Serialize(model, options);
        var modelAttribute = new
        {
            Key = "cyrus-model",
            Value = modelJson.CompressToBase64()
        };
        var source = cyrusGenerationContext.LiquidEngine.Render(modelAttribute, "ModelAttribute");
        context.AddSource("cyrus-model.g.cs", source);
    }
}