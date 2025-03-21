using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    public static class GeneratorSyntaxContextExtensions
    {
        public static IMethodSymbol? GetMethodSymbolFromContext(this GeneratorSyntaxContext context)
        {
            var recordStruct = (MethodDeclarationSyntax)context.Node;
            var model = context.SemanticModel;

            var symbol = model.GetDeclaredSymbol(recordStruct) as IMethodSymbol;
            return symbol;
        }

        public static INamedTypeSymbol? GetClassSymbolFromContext(this GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var model = context.SemanticModel;

            var symbol = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
            return symbol;
        }

        public static INamedTypeSymbol? GetRecordSymbolFromContext(this GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (RecordDeclarationSyntax)context.Node;
            var model = context.SemanticModel;

            var symbol = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
            return symbol;
        }
    }
}
