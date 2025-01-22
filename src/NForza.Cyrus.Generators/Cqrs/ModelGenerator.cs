using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Cqrs.WebApi;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs
{
    internal class ModelGenerator
    {
        internal static string ForHub(SignalRHubClassDefinition signalRHub, Compilation compilation)
        {
            var commands = signalRHub.Commands.Select(c => $"\"{c.Name}\"");
            var commandsAsString = string.Join(",", commands);
            return $"new ModelHubDefinition(\"{signalRHub.Name}\", [{commandsAsString}])";
        }

        internal static string ForNamedType(INamedTypeSymbol method, Compilation compilation)
        {
            string properties = GetPropertiesDeclaration(method, compilation);
            return $"new ModelTypeDefinition(\"{method.Name}\", [{properties}])";
        }

        private static string GetPropertiesDeclaration(INamedTypeSymbol method, Compilation compilation)
        {
            var propertyDeclarations = method.GetMembers()
                .Where(m => !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m is IPropertySymbol property)
                .OfType<IPropertySymbol>()
                .Select(m =>
                {
                    string type = m.Type.Name;
                    string name = m.Name;
                    (bool isEnumerable, ITypeSymbol? collectionType) = m.Type.IsCollection(compilation);
                    if (isEnumerable)
                    {
                        type = collectionType?.Name ?? type;                        
                    }
                    bool isNullable = m.Type.IsNullable(compilation);
                    return $"new ModelPropertyDefinition(\"{name}\", \"{type}\", {isEnumerable.ToString().ToLowerInvariant()}, {isNullable.ToString().ToLowerInvariant()})";
                });
            return string.Join(",", propertyDeclarations);
        }
    }
}