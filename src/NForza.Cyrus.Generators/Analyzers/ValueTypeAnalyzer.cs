﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Cyrus.Generators.Analyzers;

internal class ValueTypeAnalyzer
{
    internal void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var structDeclaration = (StructDeclarationSyntax)context.Node;
        if (structDeclaration.Kind() != SyntaxKind.RecordStructDeclaration)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ValueTypeMustBeARecordStruct,
                structDeclaration.GetLocation(),
                structDeclaration.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }
    }
}