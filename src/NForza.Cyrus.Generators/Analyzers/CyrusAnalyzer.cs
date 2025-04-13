﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Cyrus.Generators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CyrusAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            DiagnosticDescriptors.InternalCyrusError,
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

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        new TypedIdAnalyzer().AnalyzeSyntaxNode(context);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
#if DEBUG_ANALYZER
        Debugger.Launch();
#endif
        var methodSymbol = (IMethodSymbol)context.Symbol;
        try
        {
            new CommandAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new QueryAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new EventAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new MassTransitAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InternalCyrusError,
                Location.None,
                ex.Message + ": " + ex.StackTrace.Replace("\r", "").Replace("\n", ",")
                ));
            throw;
        }
    }
}