using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace NForza.Cyrus.Generators.Analyzers;

internal class CommandAnalyzer
{
    internal void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl)
            return;

        var isStruct =
            (
                typeDecl is StructDeclarationSyntax
                ||
                (typeDecl is RecordDeclarationSyntax recDecl && recDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword))
                ||
                typeDecl is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StructKeyword))
            );

        if (!isStruct)
            return;

        var symbol = context.SemanticModel.GetDeclaredSymbol(typeDecl, context.CancellationToken);
        if (symbol is null)
            return;

        if (!HasCommandAttribute(symbol))
            return;

        var location = typeDecl.Identifier.GetLocation();

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.CommandsCantBeStructs,
            location,
            symbol.Name));
    }

    private static bool HasCommandAttribute(ISymbol symbol) =>
        symbol.GetAttributes().Any(a =>
        {
            var attrName = a.AttributeClass?.Name;
            var fullName = a.AttributeClass?.ToDisplayString();
            return attrName is "CommandAttribute" or "Command" || (fullName?.EndsWith(".CommandAttribute") ?? false);
        });
}
