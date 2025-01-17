using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Cqrs
{
    internal class ModelGenerator
    {
        internal static string For(INamedTypeSymbol method)
        {
            string properties = GetPropertiesDeclaration(method);
            return $"new ModelDefinition(\"{method.Name}\", [{properties}])";
        }

        private static string GetPropertiesDeclaration(INamedTypeSymbol method)
        {
            var propertyDeclarations = method.GetMembers()
                .Where(m => !m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m is IPropertySymbol property)
                .OfType<IPropertySymbol>()
                .Select(m =>
                {
                    string type = m.Type.Name;
                    string name = m.Name;
                    return $"new PropertyModelDefinition(\"{name}\", \"{type}\")";
                });
            return string.Join(",", propertyDeclarations);
        }
    }
}