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

        if (returnType is INamedTypeSymbol tupleType &&
            tupleType.IsTupleType &&
            tupleType.TypeArguments.Length == 2 &&
            tupleType.TypeArguments[0].ToDisplayString() == "System.IO.Stream" &&
            tupleType.TypeArguments[1].SpecialType == SpecialType.System_String)
        {
            return tupleType.TypeArguments[0]; 
        }

        return returnType;
    }

    public static string GetCommandInvocation(this IMethodSymbol handler, string variableName, string serviceProviderVariable = "services", string? aggregateRootVariableName = null, string cancellationTokenVariableName = "cancellationToken")
    {
        var hasCancellationToken = handler.Parameters.Any(p => p.Type.IsCancellationToken());
        var aggregateRootVariable = aggregateRootVariableName != null ? $", {aggregateRootVariableName}" : "";
        var cancellationTokenVariable = hasCancellationToken ? $", {cancellationTokenVariableName}" : "";
        var additionalParameters = aggregateRootVariable + cancellationTokenVariable;
        var commandType = handler.Parameters[0].Type.ToFullName();
        var typeSymbol = handler.ContainingType.ToFullName();
        var returnType = handler.ReturnType;
        var returnsTask = handler.ReturnsTask();

        if (returnsTask)
        {
            return handler.IsStatic
                ? $"{typeSymbol}.{handler.Name}({variableName}{additionalParameters})"
                : $"{serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}({variableName}{additionalParameters})";
        }
        else
        {
            return handler.IsStatic
                ? $"{typeSymbol}.{handler.Name}({variableName}{additionalParameters})"
                : $"{serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}({variableName}{additionalParameters})";
        }
    }

    public static string GetEventLambda(this IMethodSymbol handler, string serviceProviderVariable = "services", string cancellationTokenVariableName = "cancellationToken")
    {
        var hasCancellationToken = handler.Parameters.Any(p => p.Type.IsCancellationToken());
        var cancellationTokenVariable = hasCancellationToken ? $", {cancellationTokenVariableName}" : "";
        var eventType = handler.Parameters[0].Type.ToFullName();
        var typeSymbol = handler.ContainingType.ToFullName();
        var returnType = handler.ReturnType;
        var returnsTask = handler.ReturnsTask();

        if (returnsTask)
        {
            return handler.IsStatic
                ? $"async msg => await {typeSymbol}.{handler.Name}(msg)"
                : $"async msg => await {serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}(msg)";
        }
        else
        {
            return handler.IsStatic
                ? $"msg => {typeSymbol}.{handler.Name}(msg)"
                : $"msg => {serviceProviderVariable}.GetRequiredService<{typeSymbol}>().{handler.Name}(msg)";
        }
    }

    public static string GetQueryInvocation(this IMethodSymbol handler, string serviceProviderVariable = "services")
    {
        var handlerClass = handler.ContainingType.ToFullName();
        var queryType = handler.Parameters[0].Type.ToFullName();
        var typeSymbol = handler.ContainingType.ToFullName();
        var returnType = handler.ReturnType;
        var returnsTask = handler.ReturnsTask();
        var cancellationTokenVariable = handler.Parameters.Any(p => p.Type.IsCancellationToken()) ? ", cancellationToken" : string.Empty;
        var handlerParameters = $"query{cancellationTokenVariable}";

        if (returnsTask)
        {
            return handler.IsStatic
                ? $"{handlerClass}.{handler.Name}({handlerParameters})"
                : $"{serviceProviderVariable}.GetRequiredService<{handlerClass}>().{handler.Name}({handlerParameters})";
        }
        else
        {
            return handler.IsStatic
                ? $"{handlerClass}.{handler.Name}({handlerParameters})"
                : $"{serviceProviderVariable}.GetRequiredService<{handlerClass}>().{handler.Name}({handlerParameters})";
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

        if (returnType.Name == "Result" &&
                returnType.ContainingNamespace.ToDisplayString() == "NForza.Cyrus.Abstractions")
            return CommandAdapterMethod.FromResult;

        if (returnType.IsTupleType && returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TupleElements.Length == 2)
            {
                var firstElement = namedTypeSymbol.TupleElements[0];
                if (firstElement.Type.Name == "Result" &&
                    firstElement.Type.ContainingNamespace.ToDisplayString() == "NForza.Cyrus.Abstractions")
                {
                    var secondElement = namedTypeSymbol.TupleElements[1];
                    if (secondElement.Type.Name == "Object")
                    {
                        return CommandAdapterMethod.FromResultAndMessage;
                    }
                    return CommandAdapterMethod.FromResultAndMessages;
                }
            }

            return CommandAdapterMethod.FromResultAndMessages;
        }
        return CommandAdapterMethod.FromObjects;
    }
}