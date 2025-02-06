using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class INamedTypeSymbolExtensions
{
    private static string[] typedIdAttributes = ["StringIdAttribute", "GuidIdAttribute", "IntIdAttribute"];

    public static bool IsTypedId(this INamedTypeSymbol symbol)
    {
        if (symbol != null)
            if (symbol.IsValueType)
                if (symbol.TypeKind == TypeKind.Struct)
                    if (symbol.GetAttributes().Any(a => typedIdAttributes.Contains(a.AttributeClass?.Name)))
                        return true;
        return false;
    }

    public static bool IsDirectlyDerivedFrom(this INamedTypeSymbol classSymbol, string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol?.BaseType;
        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
    }
}
