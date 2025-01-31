using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class ITypeSymbolExtensions
{
    private static string[] enumerableTypes = ["System.Collections.Generic.IEnumerable<T>", "System.Collections.Generic.List<T>"];

    public static (bool IsMatch, ITypeSymbol? ElementType) IsCollection(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return (true, arrayTypeSymbol.ElementType);
        }

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            string constructedFrom = namedTypeSymbol.ConstructedFrom.ToDisplayString();
            if (enumerableTypes.Contains(constructedFrom))
            {
                return (true, namedTypeSymbol.TypeArguments[0]);
            }
        }

        return (false, null);
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsValueType)
        {
            return namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        return false;
    }

    public static string GetTypeAliasOrName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
            throw new ArgumentNullException(nameof(typeSymbol));

        string alias = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

        string baseTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool baseTypeNameContainsGlobal = baseTypeName.Contains("global::");
        string typeName = baseTypeName.Replace("global::", "");

        if (!baseTypeNameContainsGlobal)
        {
            return baseTypeName;
        }

        if (alias != typeName)
        {
            return alias;
        }

        return typeSymbol.Name;
    }
}
