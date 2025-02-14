using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class ISymbolExtensions
{
    public static string ToFullName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static bool IsQueryHandler(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryHandlerAttribute");
    }
    public static bool IsQuery(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryAttribute");
    }

    public static bool IsCommandHandler(this ISymbol methodSymbol)
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

    public static bool IsEventHandler(this ISymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventHandlerAttribute");
    }
}