using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Cyrus.Generators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CyrusAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            DiagnosticDescriptors.MissingCommandAttribute,
            DiagnosticDescriptors.TooManyArgumentsForCommandHandler,
            DiagnosticDescriptors.CommandHandlerShouldHaveACommandParameter,
            DiagnosticDescriptors.MissingQueryAttribute,
            DiagnosticDescriptors.TooManyArgumentsForQueryHandler,
            DiagnosticDescriptors.QueryHandlerShouldHaveAQueryParameter,
            DiagnosticDescriptors.MissingEventAttribute,
            DiagnosticDescriptors.TooManyArgumentsForEventHandler,
            DiagnosticDescriptors.EventHandlerShouldHaveAEventParameter,
            DiagnosticDescriptors.ProjectShouldReferenceCyrusMassTransit,
            DiagnosticDescriptors.TypedIdMustBeARecordStruct
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.StructDeclaration);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var structDeclaration = (StructDeclarationSyntax)context.Node;
        if (structDeclaration.Kind() != SyntaxKind.RecordStructDeclaration)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MissingEventAttribute,
                structDeclaration.GetLocation(),
                structDeclaration.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
#if DEBUG_ANALYZER
        Debugger.Launch();
#endif
        var methodSymbol = (IMethodSymbol)context.Symbol;
        new CommandAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
        new QueryAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
        new EventAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
        new MassTransitAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
    }
}