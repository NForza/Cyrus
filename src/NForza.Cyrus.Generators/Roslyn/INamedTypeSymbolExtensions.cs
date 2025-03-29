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

    public static IEnumerable<(string Name, ITypeSymbol Type)> GetConstructorArguments(this INamedTypeSymbol namedTypeSymbol)
    {
        var constructor = namedTypeSymbol.InstanceConstructors
            .Where(c => !c.IsStatic)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (constructor == null)
            return []; 

        return constructor.Parameters
            .Select(p => (p.Name, p.Type))
            .ToList();
    }

    public static bool IsRecordType(this INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.IsRecord) return true; 

        bool hasEqualityOperators = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.UserDefinedOperator)
            .Any(m => m.Name == "op_Equality" || m.Name == "op_Inequality");

        return hasEqualityOperators;
    }

    public static string GetUnderlyingTypeOfTypedId(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Select(a => a.AttributeClass?.Name)
            .FirstOrDefault(name => name is "StringIdAttribute" or "IntIdAttribute" or "GuidIdAttribute") switch
        {
            "StringIdAttribute" => "string",
            "IntIdAttribute" => "int",
            _ => "System.Guid"
        };
    }
}