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

    public static ITypeSymbol GetQueryReturnType(this IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType;
        if (returnType is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            (namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>" ||
             namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.ValueTask<TResult>"))
        {
            return namedType.TypeArguments[0];
        }
        return returnType;
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

    public static CommandAdapterMethod GetAdapterMethodName(this IMethodSymbol handler)
    {
        var returnType = handler switch
        {
            _ when handler.ReturnsTask(out var taskResultType) && taskResultType != null => taskResultType,
            _ when handler.ReturnsTask() => null,
            _ => handler.ReturnType
        };

        if (returnType == null || returnType.SpecialType == SpecialType.System_Void)
            return CommandAdapterMethod.FromVoid;

        if (returnType.Name == "IResult" &&
                returnType.ContainingNamespace.ToDisplayString() == "Microsoft.AspNetCore.Http")
            return CommandAdapterMethod.FromIResult;

        if (returnType.IsTupleType && returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TupleElements.Length == 2)
            {
                var firstElement = namedTypeSymbol.TupleElements[0];
                if (firstElement.Type.Name == "IResult" &&
                    firstElement.Type.ContainingNamespace.ToDisplayString() == "Microsoft.AspNetCore.Http")
                {
                    var secondElement = namedTypeSymbol.TupleElements[1];
                    if (secondElement.Type.Name == "Object")
                    {
                        return CommandAdapterMethod.FromIResultAndEvent;
                    }
                    return CommandAdapterMethod.FromIResultAndEvents;
                }
            }

            return CommandAdapterMethod.FromIResultAndEvents;
        }
        return CommandAdapterMethod.FromObjects;
    }
}