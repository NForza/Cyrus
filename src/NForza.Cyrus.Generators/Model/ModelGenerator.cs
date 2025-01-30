using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Cqrs.WebApi;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Model;

internal class ModelGenerator
{
    internal static string ForHub(SignalRHubClassDefinition signalRHub, Compilation compilation)
    {
        var commands = signalRHub.Commands.Select(c => $"\"{c.Symbol.Name}\"");
        var commandsAsString = string.Join(",", commands);

        var events = signalRHub.Events.Select(e => $"\"{e.Symbol.Name}\"");
        var eventsAsString = string.Join(",", events);

        var queryReturnTypes = signalRHub.Queries.Select(q => q.ReturnType.Type).Distinct(SymbolEqualityComparer.IncludeNullability).OfType<INamedTypeSymbol>();
        var queryReturnTypeProperties = string.Join(",", queryReturnTypes.Select(t => GetPropertiesDeclaration(t, compilation)));

        var queries = signalRHub.Queries.Select(c => $"new ModelQueryDefinition(\"{c.Symbol.Name}\", new(\"{c.ReturnType.Type?.Name ?? null}\", {c.ReturnType.IsCollection.ToString().ToLower()}, {c.ReturnType.IsNullable.ToString().ToLower()}, [{queryReturnTypeProperties}]))");
        var queriesAsString = string.Join(",", queries);

        var supportTypes = GetSupportClasses(signalRHub, compilation, queryReturnTypes);
        var supportTypesAsString = string.Join(",", supportTypes.Select(t => ForNamedType(t, compilation)));
        return $"new ModelHubDefinition(\"{signalRHub.Name}\", {signalRHub.Path} ,[{commandsAsString}], [{queriesAsString}], [{eventsAsString}], [{supportTypesAsString}])";
    }

    private static List<INamedTypeSymbol> GetSupportClasses(SignalRHubClassDefinition signalRHub, Compilation compilation, IEnumerable<INamedTypeSymbol> queryReturnTypes)
    {
        var commandSupportClasses = signalRHub.Commands.SelectMany(t => GetSupportClasses(t.Symbol, compilation)).ToList();
        var eventSupportClasses = signalRHub.Events.SelectMany(t => GetSupportClasses(t.Symbol, compilation)).ToList();
        var queryReturnTypeSupportClasses = queryReturnTypes.SelectMany(t => GetSupportClasses(t, compilation)).ToList();

        return commandSupportClasses.Concat(eventSupportClasses).Concat(queryReturnTypeSupportClasses).Distinct(SymbolEqualityComparer.IncludeNullability).OfType<INamedTypeSymbol>().ToList();
    }

    private static List<INamedTypeSymbol> GetSupportClasses(INamedTypeSymbol t, Compilation compilation)
    {
        var supportClasses = new List<INamedTypeSymbol>();
        var properties = GetPropertiesForType(t);
        var propertyTypes = properties.Select(p => p.Type).OfType<INamedTypeSymbol>().Where(p => p.TypeKind == TypeKind.Enum);
        supportClasses.AddRange(propertyTypes);

        var nestedSupportClasses = properties.Select(p => p.Type).OfType<INamedTypeSymbol>().Where(p => p.TypeKind == TypeKind.Class).ToList();
        supportClasses.AddRange(nestedSupportClasses);

        return supportClasses;
    }

    internal static string ForNamedType(INamedTypeSymbol namedType, Compilation compilation)
    {
        switch(namedType.TypeKind)
        {
            case TypeKind.Enum:
                return $"new ModelTypeDefinition(\"{namedType.Name}\", [], [{string.Join(",", namedType.GetMembers().OfType<IFieldSymbol>().Select(f => $"\"{f.Name}\""))}])";
            default:
                string properties = GetPropertiesDeclaration(namedType, compilation);
                return $"new ModelTypeDefinition(\"{namedType.Name}\", [{properties}], [])";
        }
    }

    private static string GetPropertiesDeclaration(INamedTypeSymbol namedType, Compilation compilation)
    {
        var propertyDeclarations = GetPropertiesForType(namedType).Select(m =>
        {
            string type = GetTypeAliasOrName(m.Type);
            string name = m.Name;
            (bool isEnumerable, ITypeSymbol? collectionType) = m.Type.IsCollection(compilation);
            if (isEnumerable)
            {
                type = GetTypeAliasOrName(collectionType!);
            }
            bool isNullable = m.Type.IsNullable(compilation);
            return $"new ModelPropertyDefinition(\"{name}\", \"{type}\", {isEnumerable.ToString().ToLowerInvariant()}, {isNullable.ToString().ToLowerInvariant()})";
        });
        return string.Join(",", propertyDeclarations);
    }

    private static IEnumerable<IPropertySymbol> GetPropertiesForType(INamedTypeSymbol namedType)
    {
        return namedType.GetMembers()
                    .Where(m => !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m is IPropertySymbol property)
                    .OfType<IPropertySymbol>();
    }

    public static string GetTypeAliasOrName(ITypeSymbol typeSymbol)
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