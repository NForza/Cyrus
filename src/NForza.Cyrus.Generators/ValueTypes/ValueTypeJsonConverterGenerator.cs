using System;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.ValueTypes;

public class ValueTypeJsonConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var typedIds = cyrusGenerationContext.ValueTypes;
        foreach (var typedId in typedIds)
        {
            var sourceText = GenerateJsonConverterForTypedId(typedId, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{typedId.Name}JsonConverter.g.cs", sourceText);
        };
    }

    private string GenerateJsonConverterForTypedId(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.GetNameOrEmpty();
        var underlyingTypeName = item.GetUnderlyingTypeOfValueType();

        string? templateName = underlyingTypeName switch
        {
            "System.Guid" => "GuidValueJsonConverter",
            "string" => "StringValueJsonConverter",
            "int" => "IntValueJsonConverter",
            _ => throw new NotSupportedException($"Underlying type {underlyingTypeName} is not supported.")
        };

        var model = new
        {
            item.Name,
            Namespace = fullyQualifiedNamespace,
        };
        var source = liquidEngine.Render(model, templateName);
        return source;
    }
}