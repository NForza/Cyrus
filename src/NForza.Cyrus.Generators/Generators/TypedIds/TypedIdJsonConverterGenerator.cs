using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.Generators.TypedIds;

[Generator]
public class TypedIdJsonConverterGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var incrementalValuesProvider = context.SyntaxProvider
                     .CreateSyntaxProvider(
                         predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode) || IsRecordWithGuidIdAttribute(syntaxNode) || IsRecordWithIntIdAttribute(syntaxNode),
                         transform: (context, _) => GetNamedTypeSymbolFromContext(context));
        var typedIdsProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(typedIdsProvider, (spc, typedIds) =>
        {
            foreach (var typedId in typedIds)
            {
                var sourceText = GenerateJsonConverterForTypedId(typedId);
                spc.AddSource($"{typedId.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }

    private string GenerateJsonConverterForTypedId(INamedTypeSymbol item)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item);

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