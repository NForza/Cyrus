using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Analyzers;

internal class ValidatorAnalyzer : CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        bool ReturnsIEnumerableOfString(IMethodSymbol methodSymbol)
        {
            var ienumerableOfString = context.Compilation
                .GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")
                ?.Construct(context.Compilation.GetSpecialType(SpecialType.System_String));

            return SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, ienumerableOfString);
        }

        var isEventHandler = methodSymbol.IsValidator();

        if (!isEventHandler)
            return;

        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation() ?? Location.None;

        if (methodSymbol.Parameters.Length == 0)
        {
            location = methodSymbol.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.IncorrectNumberOfArgumentsForValidator,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        location = methodSymbol.Locations.FirstOrDefault() ?? Location.None;

        if (methodSymbol.Parameters.Length > 1)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.IncorrectNumberOfArgumentsForValidator,
                location,
                methodSymbol.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        if (!ReturnsIEnumerableOfString(methodSymbol))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ValidatorsShouldReturnIEnumerableOfString,
                location,
                methodSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
