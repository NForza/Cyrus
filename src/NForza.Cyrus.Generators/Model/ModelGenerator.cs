using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            Commands = GetCommands(cyrusGenerationContext),
            Integers = GetInts(cyrusGenerationContext.IntValues),
            Doubles = GetDoubles(cyrusGenerationContext.DoubleValues),
            Guids = GetGuids(cyrusGenerationContext.GuidValues),
            Strings = GetStrings(cyrusGenerationContext.StringValues)
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

    private IEnumerable<string> GetStrings(ImmutableArray<INamedTypeSymbol> stringValues) => stringValues.Select(s => s.Name);

    private IEnumerable<string> GetGuids(ImmutableArray<INamedTypeSymbol> guidValues) => guidValues.Select(g => g.Name);

    private IEnumerable<string> GetDoubles(ImmutableArray<INamedTypeSymbol> doubleValues) => doubleValues.Select(s => s.Name);

    private IEnumerable<string> GetInts(ImmutableArray<INamedTypeSymbol> intValues) => intValues.Select(i => i.Name);

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