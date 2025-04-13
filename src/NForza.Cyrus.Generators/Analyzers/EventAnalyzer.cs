using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Analyzers;

internal class EventAnalyzer : CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var isEventHandler = methodSymbol.IsEventHandler();

        if (!isEventHandler)
            return;

        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation() ?? Location.None;

        if (methodSymbol.Parameters.Length == 0)
        {
            location = methodSymbol.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForEventHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        location = methodSymbol.Parameters[0].Locations.FirstOrDefault() ?? Location.None;

        if (methodSymbol.Parameters.Length > 1)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForEventHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var commandParam = methodSymbol.Parameters[0].Type;
        var hasEventAttr = commandParam.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "EventAttribute"
                      || attr.AttributeClass?.ToDisplayString() == "NForza.Cyrus.Abstractions");

        if (!hasEventAttr)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MissingEventAttribute,
                location,
                commandParam.Name);

            context.ReportDiagnostic(diagnostic);
        }

    }
}
