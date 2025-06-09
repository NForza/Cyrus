using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Analyzers;

internal class QueryAnalyzer : CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var isQueryHandler = methodSymbol.IsQueryHandler();

        if (!isQueryHandler)
            return;

        if (methodSymbol.Parameters.Length == 0)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForQueryHandler,
                methodSymbol.Parameters[0].Locations.FirstOrDefault() ?? Location.None,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var location = methodSymbol.Parameters[0].Locations.FirstOrDefault() ?? Location.None;

        if (methodSymbol.Parameters.Length > 1 && !methodSymbol.Parameters[1].Type.IsCancellationToken())
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForQueryHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var commandParam = methodSymbol.Parameters[0].Type;
        var hasQueryAttr = commandParam.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "QueryAttribute"
                      || attr.AttributeClass?.ToDisplayString() == "NForza.Cyrus.Abstractions");

        if (!hasQueryAttr)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MissingQueryAttribute,
                location,
                commandParam.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
