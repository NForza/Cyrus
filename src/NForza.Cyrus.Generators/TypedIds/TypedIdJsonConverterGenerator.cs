using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.TypedIds;

public class TypedIdJsonConverterGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var typedIds = cyrusProvider.TypedIds;
        foreach (var typedId in typedIds)
        {
            var sourceText = GenerateJsonConverterForTypedId(typedId);
            spc.AddSource($"{typedId.Name}JsonConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        };
    }

    private string GenerateJsonConverterForTypedId(INamedTypeSymbol item)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
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
        var source = LiquidEngine.Render(model, templateName);
        return source;
    }
}