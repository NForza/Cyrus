﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdJsonConverterGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var incrementalValuesProvider = context.SyntaxProvider
                     .CreateSyntaxProvider(
                         predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode) || IsRecordWithGuidIdAttribute(syntaxNode),
                         transform: (context, _) => GetSemanticTargetForGeneration(context));
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

    private INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var recordStruct = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        return model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
    }

    private string GenerateJsonConverterForTypedId(INamedTypeSymbol item)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item);

        string? getMethodName = underlyingTypeName switch
        {
            "System.Guid" => "GetGuid",
            "string" => "GetString",
            _ => throw new NotSupportedException($"Underlying type {underlyingTypeName} is not supported.")
        };

        var replacements = new Dictionary<string, string>
        {
            ["TypedIdName"] = item.Name,
            ["NamespaceName"] = fullyQualifiedNamespace,
            ["GetMethodName"] = getMethodName
        };
        var source = TemplateEngine.ReplaceInResourceTemplate("JsonConverter.cs", replacements);
        return source;
    }
}