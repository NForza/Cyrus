using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs
{
    internal class ModelGenerator
    {
        internal static string For(INamedTypeSymbol method, Compilation compilation)
        {
            string properties = GetPropertiesDeclaration(method, compilation);
            return $"new ModelDefinition(\"{method.Name}\", [{properties}])";
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
                    return $"new PropertyModelDefinition(\"{name}\", \"{type}\", {isEnumerable.ToString().ToLowerInvariant()}, {isNullable.ToString().ToLowerInvariant()})";
                });
            return string.Join(",", propertyDeclarations);
        }
    }
}