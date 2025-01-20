using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class ITypeSymbolExtensions
{
    public static (bool IsMatch, ITypeSymbol? ElementType) IsCollection(this ITypeSymbol typeSymbol, Compilation compilation)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return (true, arrayTypeSymbol.ElementType);
        }

        var enumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        var listSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

        if (enumerableSymbol == null || listSymbol == null)
        {
            return (false, null); 
        }

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.OriginalDefinition.Equals(enumerableSymbol, SymbolEqualityComparer.Default))
            {
                return (true, namedTypeSymbol.TypeArguments[0]);
            }

            if (namedTypeSymbol.OriginalDefinition.Equals(listSymbol, SymbolEqualityComparer.Default))
            {
                return (true, namedTypeSymbol.TypeArguments[0]);
            }
        }

        return (false, null);
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol, Compilation compilation)
    {
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsValueType)
        {
            var nullableSymbol = compilation.GetTypeByMetadataName("System.Nullable`1");
            return namedType.OriginalDefinition.Equals(nullableSymbol, SymbolEqualityComparer.Default);
        }

        return false;
    }
}
