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

    public static bool IsLocalEvent(this INamedTypeSymbol namedTypeSymbol)
    {
        AttributeData? eventAttribute = namedTypeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "EventAttribute");
        if (eventAttribute != null)
        {
            KeyValuePair<string, TypedConstant> localValue = eventAttribute.NamedArguments.FirstOrDefault(na => na.Key == "Local");
            if (localValue.Value.Kind == TypedConstantKind.Primitive && localValue.Value.Value is bool localValueBool)
            {
                return localValueBool;
            }
        }
        return false;
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

    public static string GetUnderlyingTypeOfValueType(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Select(a => a.AttributeClass?.Name)
            .FirstOrDefault(name => name is "StringValueAttribute" or "IntValueAttribute" or "GuidValueAttribute") switch
        {
            "StringValueAttribute" => "string",
            "IntValueAttribute" => "int",
            _ => "System.Guid"
        };
    }

    public static bool HasAttribute(this ISymbol typeSymbol, string attributeName) 
        => typeSymbol
                .GetAttributes()
                .Select(a => a.AttributeClass?.Name ?? "")
                .Any(name => name == attributeName || name == attributeName + "Attribute");

    public static bool IsCancellationToken(this INamedTypeSymbol typeSymbol)
        => typeSymbol.Name == "CancellationToken" 
           && typeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading";

    public static bool IsAggregateRoot(this ISymbol typeSymbol) 
        => typeSymbol.HasAttribute("AggregateRoot");

    public static IPropertySymbol? GetAggregateRootIdProperty(this INamedTypeSymbol typeSymbol)
    {
        var properties = typeSymbol.GetPublicProperties();
        var rootIdProperty = properties.FirstOrDefault(p =>
        {
            var attributes = p.GetAttributes();
            return attributes.Any(a => a.AttributeClass?.Name.StartsWith("AggregateRootId") ?? false);
        });
        return rootIdProperty ;
    }
}