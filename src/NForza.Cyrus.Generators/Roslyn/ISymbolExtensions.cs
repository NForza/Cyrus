using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class ISymbolExtensions
{
    public static string ToFullName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
