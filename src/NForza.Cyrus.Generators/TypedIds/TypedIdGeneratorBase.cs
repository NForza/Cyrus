using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

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
