using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    internal static class MethodSymbolExtensions
    {
        public static bool ReturnsCommandResult(this IMethodSymbol methodSymbol)
        {
            ITypeSymbol returnType = methodSymbol.ReturnType;

            if (returnType.Name == "CommandResult")
                return true;

            if (returnType is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
            {
                if (namedTypeSymbol.OriginalDefinition.ToString() == "System.Threading.Tasks.Task<T>")
                {
                    var genericArg = namedTypeSymbol.TypeArguments[0];
                    return genericArg.Name == "CommandResult";
                }
            }

            return false;
        }

        public static bool IsAsync(this IMethodSymbol handler)
        {
            var returnType = handler.ReturnType;
            var isAsync = returnType.Name == "Task" || returnType.Name == "ValueTask";
            return isAsync;
        }

        public static string GetCommandInvocation(this IMethodSymbol handler, string variableName, string serviceProviderVariable = "services")
        {
            var commandType = handler.Parameters[0].Type.ToFullName();
            var typeSymbol = handler.ContainingType.ToFullName();
            var returnType = handler.ReturnType;
            var isAsync = handler.IsAsync();    

            if (isAsync)
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
            var isAsync = handler.IsAsync();

            if (isAsync)
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

        public static string GetCommandHandlerRoute(this IMethodSymbol methodSymbol)
        {
            var firstParam = methodSymbol.Parameters[0].Type;
            return firstParam.GetCommandRoute();
        }

        public static string GetQueryHandlerRoute(this IMethodSymbol methodSymbol)
        {
            var firstParam = methodSymbol.Parameters[0].Type;
            return firstParam.GetQueryRoute();
        }

        public static string GetCommandHandlerVerb(this IMethodSymbol methodSymbol)
        {
            var firstParam = methodSymbol.Parameters[0].Type;
            return firstParam.GetCommandVerb();
        }

        public static INamedTypeSymbol GetCommandType(this IMethodSymbol methodSymbol)
        {
            var firstParam = methodSymbol.Parameters[0].Type;
            return (INamedTypeSymbol)firstParam;
        }

        public static bool HasCommandBody(this IMethodSymbol methodSymbol)
        {
            var command = methodSymbol.GetCommandType();
            var routeParameters = RouteParameterDiscovery.FindAllParametersInRoute(command.GetCommandRoute()).Select(p => p.Name);
            return command.GetPublicProperties().Where(p => !routeParameters.Contains(p.Name)).Any();
        }
    }
}
