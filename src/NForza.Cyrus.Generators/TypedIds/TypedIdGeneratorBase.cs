using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.TypedIds.Generator;

public abstract class TypedIdGeneratorBase : CyrusGeneratorBase
{
    protected static bool IsRecordWithAttribute(SyntaxNode syntaxNode, string attributeName)
    {
        bool isRecordWithStringId = syntaxNode is RecordDeclarationSyntax recordDeclaration &&
                       recordDeclaration.HasAttribute(attributeName);
        return isRecordWithStringId;
    }

    protected bool IsRecordWithGuidIdAttribute(SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "GuidId");

    protected bool IsRecordWithIntIdAttribute(SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "IntId");

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
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol && typeSymbol.IsTypedId())
                {
                    yield return typeSymbol;
                }
            }
        }
    }

    protected string GetUnderlyingTypeOfTypedId(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Select(a => a.AttributeClass?.Name)
            .FirstOrDefault(name => name is "StringIdAttribute" or "IntIdAttribute" or "GuidIdAttribute") switch
        {
            "StringIdAttribute" => "string",
            "IntIdAttribute" => "int",
            _ => "System.Guid"
        };
    }

    protected INamedTypeSymbol? GetNamedTypeSymbolFromContext(GeneratorSyntaxContext context)
    {
        var recordStruct = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
        return symbol;
    }
}
