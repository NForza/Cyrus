using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.WebApi;

internal static class TypeAnnotations
{
    public static string AugmentRouteWithTypeAnnotations(string path, ITypeSymbol typeSymbol)
    {
        return Regex.Replace(path, @"\{(?<param>\w+)\}", match =>
        {
            var paramName = match.Groups["param"].Value;

            var property = typeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));

            if (property is null)
                return match.Value;

            var constraint = GetRouteConstraint(property.Type);
            return constraint is null
                ? match.Value
                : $"{{{paramName}:{constraint}}}";
        });
    }

    static string? GetRouteConstraint(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                && namedType.TypeArguments.Length == 1)
            {
                return GetRouteConstraint(namedType.TypeArguments[0]);
            }

            var attributes = namedType.GetAttributes();

            if (attributes.Any(a => a.AttributeClass?.Name == "GuidValueAttribute"))
                return "guid";

            if (attributes.Any(a => a.AttributeClass?.Name == "IntValueAttribute"))
                return "int";

            if (attributes.Any(a => a.AttributeClass?.Name == "StringValueAttribute"))
                return null;

            var typeName = namedType.ToDisplayString();

            return typeName switch
            {
                "System.Guid" => "guid",
                "System.Int32" => "int",
                "System.Int64" => "long",
                "System.Boolean" => "bool",
                "System.Single" => "float",
                "System.Double" => "double",
                _ => null
            };
        }

        return null;
    }
}