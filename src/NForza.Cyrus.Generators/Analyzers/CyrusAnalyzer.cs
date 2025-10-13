using System;
#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
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
            DiagnosticDescriptors.IncorrectNumberOfArgumentsForEventHandler,
            DiagnosticDescriptors.EventHandlerShouldHaveAEventParameter,
            DiagnosticDescriptors.ValueTypeMustBeARecordStruct,
            DiagnosticDescriptors.IncorrectNumberOfArgumentsForValidator,
            DiagnosticDescriptors.ValidatorsShouldReturnIEnumerableOfString,
            DiagnosticDescriptors.ResultTupleElementShouldBeCalledResult,
            DiagnosticDescriptors.MessagesTupleElementShouldBeCalledMessages,
            DiagnosticDescriptors.CommandHandlerArgumentShouldBeAnAggregateRoot,
            DiagnosticDescriptors.CommandForCommandHandlerShouldHaveAggregateRootIdProperty,
            DiagnosticDescriptors.UnrecognizedParameterForCommandHandler,
            DiagnosticDescriptors.AggregateRootShouldHaveAggregateRootIdProperty,
            DiagnosticDescriptors.CommandsCantBeStructs,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.StructDeclaration, SyntaxKind.RecordStructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.ClassDeclaration );
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        new ValueTypeAnalyzer().AnalyzeSyntaxNode(context);
        new CommandAnalyzer().AnalyzeSyntaxNode(context);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        try
        {
            new CommandHandlerAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new QueryAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new EventAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new MassTransitAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
            new ValidatorAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
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