using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class ISymbolExtensions
{
    private const string defaultCommandVerb = "Post";

    public static string ToFullName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static bool IsQuery(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryAttribute");
    }

    public static string GetQueryRoute(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "QueryHandlerAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                return attr.NamedArguments[0].Value.Value?.ToString() ?? "";
            }
        }
        return string.Empty;
    }

    public static string? GetCommandRoute(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "CommandHandlerAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                return attr.NamedArguments.FirstOrDefault( kvp => kvp.Key == "Route").Value.Value?.ToString() ?? null;
            }
        }
        return null;
    }

    public static bool HasCommandRoute(this ISymbol methodSymbol)
        => GetCommandRoute(methodSymbol) != null;
    public static string GetCommandVerb(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "CommandHandlerAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                TypedConstant argument = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Verb").Value;
                if (argument.IsNull)
                {
                    return defaultCommandVerb;
                }
                var member = argument.Type!.GetMembers()
                     .First(m => m is IFieldSymbol field && field.HasConstantValue && Equals(field.ConstantValue, argument.Value));

                return argument.IsNull ? defaultCommandVerb : member.Name;
            }
        }
        return defaultCommandVerb;
    }

    public static bool IsCommand(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "CommandAttribute");
    }

    public static bool IsEvent(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventAttribute");
    }
}