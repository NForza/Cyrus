using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class INamedTypeSymbolExtensions
{
    public static bool IsDirectlyDerivedFrom(this INamedTypeSymbol classSymbol, string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol?.BaseType;
        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
    }

    private static string[] assembliesToSkip = ["System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle", "RabbitMQ", "MassTransit"];
    public static IEnumerable<INamedTypeSymbol> GetAllTypesRecursively(this INamespaceSymbol namespaceSymbol)
    {
        var assemblyName = namespaceSymbol?.ContainingAssembly?.Name;
        if (assemblyName != null && assembliesToSkip.Any(n => assemblyName.StartsWith(n)))
        {
            return [];
        }

        var types = namespaceSymbol!.GetTypeMembers();
        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            types = types.AddRange(GetAllTypesRecursively(subNamespace));
        }
        return types;
    }

    public static IEnumerable<IPropertySymbol> GetPublicProperties(this INamedTypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public);
    }
}
