using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Generators.SignalR;

namespace NForza.Cyrus.Generators.Model;

internal class ModelGenerator
{
    internal static string ForHub(SignalRHubClassDefinition signalRHub)
    {
        var commands = signalRHub.Commands.Select(c => $"\"{c.Name}\"");
        var commandsAsString = string.Join(",", commands);

        var events = signalRHub.Events.Select(e => $"\"{e.Name}\"");
        var eventsAsString = string.Join(",", events);

        var queryReturnTypes = signalRHub.Queries.Select(q => q.ReturnType.Type).Distinct(SymbolEqualityComparer.IncludeNullability).OfType<INamedTypeSymbol>();
        var queryReturnTypeProperties = string.Join(",", queryReturnTypes.Select(t => GetPropertiesDeclaration(t)));
        var queries = signalRHub.Queries.Select(c => $"new ModelQueryDefinition(\"{c.Name}\", new ModelTypeDefinition(\"{c.ReturnType.Type?.Name ?? null}\", [{queryReturnTypeProperties}], {c.ReturnType.Type?.IsCollection().IsMatch.ToString().ToLower() ?? "false"}, {c.ReturnType.Type?.IsNullable().ToString().ToLower() ?? "false"}, []))");
        var queriesAsString = string.Join(",", queries);

        return $"new ModelHubDefinition(\"{signalRHub.Name}\", {signalRHub.Path} ,[{commandsAsString}], [{queriesAsString}], [{eventsAsString}])";
    }

    internal static string ForNamedType(INamedTypeSymbol namedType)
    {
        string properties = GetPropertiesDeclaration(namedType);
        return $"new ModelTypeDefinition(\"{namedType.Name}\", [{properties}], {namedType.IsCollection().IsMatch.ToString().ToLower()}, {namedType.IsNullable().ToString().ToLower()}, [])";
    }

    private static string GetPropertiesDeclaration(INamedTypeSymbol namedType)
    {
        var propertyDeclarations = namedType.GetMembers()
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
                var model = new
                {
                    Name = name,
                    Type = type,
                    IsEnumerable = isEnumerable,
                    IsNullable = isNullable
                };
                return ScribanEngine.Render("model-property", model);
            });
        return string.Join(",", propertyDeclarations);
    }
}