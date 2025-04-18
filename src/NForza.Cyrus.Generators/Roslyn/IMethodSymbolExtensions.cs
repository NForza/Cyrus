using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class IMethodSymbolExtensions
{
    public static bool IsCommandHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "CommandHandlerAttribute");
    }

    public static bool IsValidator(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "ValidatorAttribute");
    }

    public static bool IsEventHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "EventHandlerAttribute");
    }

    public static bool IsQueryHandler(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "QueryHandlerAttribute");
    }

    public static bool ReturnsTask(this IMethodSymbol methodSymbol)
        => ReturnsTask(methodSymbol, out _);

    public static bool ReturnsTask(this IMethodSymbol methodSymbol, out ITypeSymbol? taskResultType)
    {
        taskResultType = null;
        if (methodSymbol == null)
        {
            throw new ArgumentNullException(nameof(methodSymbol));
        }

        if (!(methodSymbol.ReturnType is INamedTypeSymbol namedReturn))
        {
            return false;
        }

        if (namedReturn.ContainingNamespace?.ToDisplayString() != "System.Threading.Tasks" ||
            namedReturn.Name != "Task")
        {
            return false;
        }

        if (!namedReturn.IsGenericType)
        {
            return true;
        }

        if (namedReturn.TypeArguments.Length == 1)
        {
            taskResultType = namedReturn.TypeArguments[0];
            return true;
        }

        return false;
    }

    public static string GetCommandInvocation(this IMethodSymbol handler, string variableName, string serviceProviderVariable = "services")
    {
        var commandType = handler.Parameters[0].Type.ToFullName();
        var typeSymbol = handler.ContainingType.ToFullName();
        var returnType = handler.ReturnType;
        var returnsTask = handler.ReturnsTask();

        if (returnsTask)
        {
            return handler.IsStatic
                ? $"{typeSymbol}.{handler.Name}({variableName})"
                : $"{serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}({variableName})";
        }
        else
        {
            return handler.IsStatic
                ? $"{typeSymbol}.{handler.Name}({variableName})"
                : $"{serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}({variableName})";
        }
    }

    public static string GetQueryInvocation(this IMethodSymbol handler, string serviceProviderVariable = "services")
    {
        var handlerClass = handler.ContainingType.ToFullName();
        var queryType = handler.Parameters[0].Type.ToFullName();
        var typeSymbol = handler.ContainingType.ToFullName();
        var returnType = handler.ReturnType;
        var returnsTask = handler.ReturnsTask();

        if (returnsTask)
        {
            return handler.IsStatic
                ? $"{handlerClass}.{handler.Name}(query)"
                : $"{serviceProviderVariable}.GetRequiredService<{handlerClass}>().{handler.Name}(query)";
        }
        else
        {
            return handler.IsStatic
                ? $"{handlerClass}.{handler.Name}(query)"
                : $"{serviceProviderVariable}.GetRequiredService<{handlerClass}>().{handler.Name}(query)";
        }
    }

    public static INamedTypeSymbol GetCommandType(this IMethodSymbol methodSymbol)
    {
        var firstParam = methodSymbol.Parameters[0].Type;
        return (INamedTypeSymbol)firstParam;
    }

    public static bool HasParametersInBody(this IMethodSymbol methodSymbol)
    {
        var command = methodSymbol.GetCommandType();
        var routeParameters = RouteParameterDiscovery.FindAllParametersInRoute(command.GetCommandRoute());
        return command.GetPublicProperties().Where(p => !routeParameters.Contains(p.Name)).Any();
    }
}
