using System.Linq;
using System.Text.Json;
using Cyrus;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Model;

public class ModelGenerator : CyrusGeneratorBase
{
    public static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = false };

    public override void GenerateSource(SourceProductionContext context, CyrusGenerationContext cyrusGenerationContext)
    {
        var model = new CyrusMetadata
        {
            Commands = GetCommands(cyrusGenerationContext)
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

    private static System.Collections.Generic.IEnumerable<ModelTypeDefinition> GetCommands(CyrusGenerationContext cyrusGenerationContext)
    {
        return cyrusGenerationContext.Commands.Select(c =>
            new ModelTypeDefinition(
                c.Name,
                c.ToFullName(),
                c.Description(),
                c.GetPropertyModels(),
                c.Values(),
                c.IsCollection().IsMatch,
                c.IsNullable()
            )).ToArray();
    }
}