using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class TypedIdJsonConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var typedIds = cyrusGenerationContext.TypedIds;
        foreach (var typedId in typedIds)
        {
            var sourceText = GenerateJsonConverterForTypedId(typedId, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{typedId.Name}JsonConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateJsonConverterForTypedId(INamedTypeSymbol item, LiquidEngine liquidEngine)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.GetNameOrEmpty();
        var underlyingTypeName = item.GetUnderlyingTypeOfTypedId();

        string? templateName = underlyingTypeName switch
        {
            "System.Guid" => "GuidIdJsonConverter",
            "string" => "StringIdJsonConverter",
            "int" => "IntIdJsonConverter",
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