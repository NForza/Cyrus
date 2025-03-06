using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class ISymbolExtensions
{
    public static string ToFullName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static bool IsQueryHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryHandlerAttribute");
    }
    public static bool IsQuery(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryAttribute");
    }

    public static string GetQueryRoute(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "QueryAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                return attr.NamedArguments[0].Value.Value?.ToString() ?? "";
            }
        }
        return string.Empty;
    }

    public static string GetCommandRoute(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "CommandAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                return attr.NamedArguments.FirstOrDefault( kvp => kvp.Key == "Route").Value.Value?.ToString() ?? "";
            }
        }
        return string.Empty;
    }

    public static string GetCommandVerb(this ISymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "CommandAttribute");
        if (attr != null)
        {
            if (attr.NamedArguments.Length > 0)
            {
                TypedConstant argument = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Verb").Value;
                if (argument.IsNull)
                {
                    return "Get";
                }
                var member = argument.Type!.GetMembers()
                     .First(m => m is IFieldSymbol field && field.HasConstantValue && Equals(field.ConstantValue, argument.Value));

                return argument.IsNull ? "Get" : member.Name;
            }
        }
        return "Get";
    }

    public static bool IsCommandHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "CommandHandlerAttribute");
    }
    public static bool IsCommand(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "CommandAttribute");
    }

    public static bool IsEvent(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventAttribute");
    }

    public static bool IsEventHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventHandlerAttribute");
    }
}