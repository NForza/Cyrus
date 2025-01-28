using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Cqrs.WebApi;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Model;

internal class ModelGenerator
{
    internal static string ForHub(SignalRHubClassDefinition signalRHub, Compilation compilation)
    {
        var commands = signalRHub.Commands.Select(c => $"\"{c.Name}\"");
        var commandsAsString = string.Join(",", commands);

        var events = signalRHub.Events.Select(e => $"\"{e.Name}\"");
        var eventsAsString = string.Join(",", events);

        var queryReturnTypes = signalRHub.Queries.Select(q => q.ReturnType.Type).Distinct(SymbolEqualityComparer.IncludeNullability).OfType<INamedTypeSymbol>();
        var queryReturnTypeProperties = string.Join(",", queryReturnTypes.Select(t => GetPropertiesDeclaration(t, compilation)));
        var queries = signalRHub.Queries.Select(c => $"new ModelQueryDefinition(\"{c.Name}\", new(\"{c.ReturnType.Type?.Name ?? null}\", {c.ReturnType.IsCollection.ToString().ToLower()}, {c.ReturnType.IsNullable.ToString().ToLower()}, [{queryReturnTypeProperties}]))");
        var queriesAsString = string.Join(",", queries);

        return $"new ModelHubDefinition(\"{signalRHub.Name}\", {signalRHub.Path} ,[{commandsAsString}], [{queriesAsString}], [{eventsAsString}])";
    }

    internal static string ForNamedType(INamedTypeSymbol namedType, Compilation compilation)
    {
        string properties = GetPropertiesDeclaration(namedType, compilation);
        return $"new ModelTypeDefinition(\"{namedType.Name}\", [{properties}])";
    }

    private static string GetPropertiesDeclaration(INamedTypeSymbol namedType, Compilation compilation)
    {
        var propertyDeclarations = namedType.GetMembers()
            .Where(m => !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m is IPropertySymbol property)
            .OfType<IPropertySymbol>()
            .Select(m =>
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