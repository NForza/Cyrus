using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class SyntaxNodeExtensions
{
    public static bool IsCommandHandler(this SyntaxNode syntaxNode)
     => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.HasCommandHandlerAttribute();

    public static bool IsAggregateRoot(this SyntaxNode syntaxNode)
     => syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
        &&
        classDeclarationSyntax.HasAggregateRootAttribute();


    public static bool IsQueryHandler(this SyntaxNode syntaxNode)
        => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
           &&
           methodDeclarationSyntax.HasQueryHandlerAttribute();

    public static bool IsEventHandler(this SyntaxNode syntaxNode)
    => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.HasEventHandlerAttribute();

    public static bool IsEvent(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is RecordDeclarationSyntax classDeclaration)
        {
            bool isEvent = classDeclaration.HasEventAttribute();
            return isEvent;
        };
        return false;
    }

    public static bool IsQuery(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is RecordDeclarationSyntax classDeclaration)
        {
            bool isQuery = classDeclaration.HasQueryAttribute();
            return isQuery;
        };
        return false;
    }

    public static bool IsCommand(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is RecordDeclarationSyntax classDeclaration)
        {
            bool isCommand = classDeclaration.HasCommandAttribute();
            return isCommand;
        };
        return false;
    }

    public static bool IsRecordWithAttribute(this SyntaxNode syntaxNode, string attributeName)
    {
        bool isRecordWithAttribute = syntaxNode is RecordDeclarationSyntax recordDeclaration &&
                       recordDeclaration.HasAttribute(attributeName);
        return isRecordWithAttribute;
    }

    public static bool IsRecordWithGuidIdAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "GuidId");

    public static bool IsRecordWithIntIdAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "IntId");

    public static bool IsRecordWithStringIdAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "StringId");

}
