using System.Text.Json;
using Cyrus;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Model;

public class ModelGenerator : CyrusGeneratorBase
{
    override public void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var model = new
        {
            Id = 1,
            Name = "Cyrus Model"
        };

        var modelJson = JsonSerializer.Serialize(model);
        var modelAttribute = new
        {
            Key = "cyrus-model",
            Value = modelJson.CompressToBase64()
        };
        var source = cyrusGenerationContext.LiquidEngine.Render(modelAttribute, "ModelAttribute");
        context.AddSource("cyrus-model.g.cs", source);
    }
}