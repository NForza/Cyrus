using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class IPropertySymbolExtensions
{
    public static string? GetDefaultValue(this IPropertySymbol property)
    {
        foreach (var syntaxRef in property.DeclaringSyntaxReferences)
        {
            var node = syntaxRef.GetSyntax();

            switch (node)
            {
                case PropertyDeclarationSyntax prop:
                    if (prop.Initializer is { } init)
                        return init.Value.ToFullString().Trim();

                    if (prop.ExpressionBody is { } arrow)
                        return arrow.Expression.ToFullString().Trim();
                    break;

                case ParameterSyntax param when param.Default is { } def:   
                    return def.Value.ToFullString().Trim();               
            }
        }

        var attr = property.GetAttributes()
                           .FirstOrDefault(a => a.AttributeClass?.Name ==
                                                nameof(DefaultValueAttribute));

        if (attr is { ConstructorArguments.Length: > 0 })
            return attr.ConstructorArguments[0].Value?.ToString();

        return null;
    }
}
