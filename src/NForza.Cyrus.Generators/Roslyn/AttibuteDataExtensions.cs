using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class AttibuteDataExtensions
{
    private static string[] excludedNamespaces = ["System", "NForza"];

    public static string? ToNewInstanceString(this AttributeData attribute)
    {
        return attribute.AttributeClass != null && !excludedNamespaces.Any(ns => attribute.AttributeClass.ContainingNamespace.ToDisplayString().StartsWith(ns))
            ? GenerateAttributeInstanceCode(attribute)
            : null;
    }

    static string GenerateAttributeInstanceCode(AttributeData attribute)
    {
        string FormatTypedConstant(TypedConstant constant)
        {
            if (constant.Kind == TypedConstantKind.Array)
            {
                var elements = constant.Values.Select(FormatTypedConstant);
                return $"new[] {{ {string.Join(", ", elements)} }}";
            }

            if (constant.Type?.TypeKind == TypeKind.Enum)
            {
                return $"{constant.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{constant.Value}";
            }

            return constant.Type.SpecialType switch
            {
                SpecialType.System_String => $"\"{constant.Value}\"",
                SpecialType.System_Char => $"'{constant.Value}'",
                SpecialType.System_Boolean => constant.Value?.ToString()!.ToLower(),
                _ => constant.Value?.ToString() ?? "null"
            };
        }

        var typeName = attribute.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Constructor arguments
        var ctorArgs = attribute.ConstructorArguments
            .Select(FormatTypedConstant)
            .ToList();

        var ctorArgString = ctorArgs.Count > 0
            ? $"({string.Join(", ", ctorArgs)})"
            : "()";

        // Named arguments (e.g., Roles = "Admin")
        var namedArgs = attribute.NamedArguments
            .Select(kvp => $"{kvp.Key} = {FormatTypedConstant(kvp.Value)}")
            .ToList();

        var initializer = namedArgs.Count > 0
            ? $" {{ {string.Join(", ", namedArgs)} }}"
            : "";

        return $"new {typeName}{ctorArgString}{initializer}";
    }


}
