using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    public static class SyntaxNodeExtensions
    {
        public static bool IsCommandHandler(this SyntaxNode syntaxNode)
         => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
            &&
            methodDeclarationSyntax.HasAttribute("CommandHandler");

        public static bool IsQueryHandler(this SyntaxNode syntaxNode)
            => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
               &&
               methodDeclarationSyntax.HasAttribute("QueryHandler");

        public static bool IsEventHandler(this SyntaxNode syntaxNode)
        => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
            &&
            methodDeclarationSyntax.HasAttribute("EventHandler");

        public static bool IsEvent(this SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax classDeclaration)
            {
                bool isEvent = classDeclaration.Identifier.Text.EndsWith("Event");
                return isEvent;
            };
            return false;
        }

        public static bool IsQuery(this SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax classDeclaration)
            {
                bool isEvent = classDeclaration.HasAttribute("Query");
                return isEvent;
            };
            return false;
        }
    }
}
