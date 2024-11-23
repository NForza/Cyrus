﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Generators;

namespace NForza.Lumia.TypedIds.Generator;

public abstract class TypedIdGeneratorBase : IncrementalGeneratorBase
{
    protected static bool IsRecordWithAttribute(SyntaxNode syntaxNode, string attributeName)
    {
        bool isRecordWithStringId = syntaxNode is RecordDeclarationSyntax recordDeclaration &&
                       recordDeclaration.HasAttribute(attributeName);
        return isRecordWithStringId;
    }

    protected bool IsRecordWithGuidIdAttribute(SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "GuidId");

    protected static bool IsRecordWithStringIdAttribute(SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "StringId");

    protected IEnumerable<INamedTypeSymbol> GetAllTypedIds(Compilation compilation, string typedIdName)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var typeDeclarations = syntaxTree.GetRoot().DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol && typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct)
                {
                    if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == typedIdName))
                        yield return typeSymbol;
                }
            }
        }
    }

    protected string GetUnderlyingTypeOfTypedId(INamedTypeSymbol typeSymbol)
    {
        var hasStringIdProperty = typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "StringIdAttribute");
        if (hasStringIdProperty)
            return "string";
        return "System.Guid";
    }

    protected INamedTypeSymbol? GetNamedTypeSymbolFromContext(GeneratorSyntaxContext context)
    {
        var recordStruct = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
        return symbol;
    }
}