using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Cyrus.Generators.Analyzers;

internal abstract class CyrusAnalyzerBase
{
    public abstract void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol);
}
