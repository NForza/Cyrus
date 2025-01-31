using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Model;

public static class NamespaceSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol @namespace)
    {
        foreach (var member in @namespace.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var nestedType in nestedNamespace.GetAllTypes())
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