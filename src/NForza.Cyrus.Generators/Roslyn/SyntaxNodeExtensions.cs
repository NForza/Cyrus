using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    public static class SyntaxNodeExtensions
    {
        public static bool IsCommandHandler(this SyntaxNode syntaxNode)
         => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
            &&
            methodDeclarationSyntax.HasCommandHandlerAttribute();

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
                bool isEvent = classDeclaration.HasQueryAttribute();
                return isEvent;
            };
            return false;
        }
    }
}
