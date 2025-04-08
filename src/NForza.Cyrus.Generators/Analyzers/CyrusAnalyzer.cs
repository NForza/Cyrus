using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        new CommandAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);
        new QueryAnalyzer().AnalyzeMethodSymbol(context, methodSymbol);

    }
}