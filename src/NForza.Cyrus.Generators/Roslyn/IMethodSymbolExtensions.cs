using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    public static class IMethodSymbolExtensions
    {
        public static bool IsQueryHandler(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryHandlerAttribute");
        }

        public static bool IsCommandHandler(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "CommandHandlerAttribute");
        }

        public static bool IsEventHandler(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventHandlerAttribute");
        }

    }
}
