using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Analyzers;

internal class CommandAnalyzer: CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var isCommandHandler = methodSymbol.IsCommandHandler();

        if (!isCommandHandler)
            return;

        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation()
                       ?? Location.None;

        if (methodSymbol.Parameters.Length == 0)
        {
            location = methodSymbol.Parameters[0].Locations.FirstOrDefault()
                       ?? Location.None;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForCommandHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        location = methodSymbol.Parameters[0].Locations.FirstOrDefault() ?? Location.None;

        if (methodSymbol.Parameters.Length > 1)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TooManyArgumentsForCommandHandler,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var commandParam = methodSymbol.Parameters[0].Type;
        var hasCommandAttr = commandParam.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "CommandAttribute"
                      || attr.AttributeClass?.ToDisplayString() == "NForza.Cyrus.Abstractions");

        if (!hasCommandAttr)
        {

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MissingCommandAttribute,
                location,
                commandParam.Name);

            context.ReportDiagnostic(diagnostic);
        }

    }
}
