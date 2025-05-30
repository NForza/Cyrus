using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Abstractions.Model;

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

    public static ModelPropertyDefinition[] GetPropertyModels(this ITypeSymbol typeSymbol)
    {
        var propertyDeclarations = typeSymbol.GetMembers()
            .Where(m => !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m is IPropertySymbol property)
            .OfType<IPropertySymbol>()
            .Select(m =>
            {
                string type = m.Type.GetTypeAliasOrName();
                string name = m.Name;
                (bool isEnumerable, ITypeSymbol? collectionType) = m.Type.IsCollection();
                if (isEnumerable)
                {
                    type = collectionType!.GetTypeAliasOrName();
                }
                bool isNullable = m.Type.IsNullable();
                var model = new ModelPropertyDefinition(name, type, isEnumerable, isNullable);
                return model;
            });
        return propertyDeclarations.ToArray();
    }

    public static IEnumerable<INamedTypeSymbol> GetReferencedTypes(this ITypeSymbol typeSymbol)
    {
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var result = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        typeSymbol.CollectReferencedTypesFromPublicProperties(visited, result, isRoot: true);

        return result.OfType<INamedTypeSymbol>();
    }

    private static void CollectReferencedTypesFromPublicProperties(
        this ITypeSymbol typeSymbol,
        HashSet<ITypeSymbol> visited,
        HashSet<ITypeSymbol> result,
        bool isRoot)
    {
        if (typeSymbol == null || visited.Contains(typeSymbol))
            return;

        visited.Add(typeSymbol);

        if (!isRoot && !typeSymbol.ContainingAssembly.IsFrameworkAssembly() && !typeSymbol.IsKnownType())
            result.Add(typeSymbol);

        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => p.DeclaredAccessibility == Accessibility.Public))
        {
            property.Type.CollectReferencedTypesFromPublicProperties(visited, result, isRoot: false);
        }
    }

    public static bool IsTaskType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        return namedType.Name == "Task" &&
                namedType.OriginalDefinition.Name.StartsWith("Task");
    }

    public static (bool isTaskType, ITypeSymbol? typeSymbol) GetTaskType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return (false, null); 
        }
        if (!namedTypeSymbol.IsTaskType())
            return (false, null);

        return namedTypeSymbol.TypeArguments.Length == 1
            ? (true, namedTypeSymbol.TypeArguments[0])
            : (false, null);
    }

    public static bool IsVoid(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString() == "void";
    }

    private static bool IsKnownType(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsQuery() || typeSymbol.IsCommand() || typeSymbol.IsEvent() || typeSymbol.IsTypedId();
    }

    private static string[] typedIdAttributes = ["StringIdAttribute", "GuidIdAttribute", "IntIdAttribute"];

    public static bool IsTypedId(this ITypeSymbol symbol)
    {
        if (symbol != null)
            if (symbol.IsValueType)
                if (symbol.TypeKind == TypeKind.Struct)
                    if (symbol.GetAttributes().Any(a => typedIdAttributes.Contains(a.AttributeClass?.Name)))
                        return true;
        return false;
    }
}