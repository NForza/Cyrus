using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class INamedTypeSymbolExtensions
{
    public static bool IsDirectlyDerivedFrom(this INamedTypeSymbol classSymbol, string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol?.BaseType;
        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
    }
}
