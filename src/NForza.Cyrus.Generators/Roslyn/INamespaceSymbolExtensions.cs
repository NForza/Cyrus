using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class INamespaceSymbolExtensions
{

    public static string GetNameOrEmpty(this INamespaceSymbol symbol)
    {
        return symbol.IsGlobalNamespace ? string.Empty : symbol.ToDisplayString();
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var nestedType in GetAllTypes(nestedNamespace))
                {
                    yield return nestedType;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                yield return namedType;
            }
        }
    }
}
