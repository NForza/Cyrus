using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public static string GetCommandInvocation(this IMethodSymbol handler)
        {
            var commandType = handler.Parameters[0].Type.ToFullName();
            var typeSymbol = handler.ContainingType.ToFullName();
            var returnType = handler.ReturnType;
            var isAsync = handler.IsAsync();    

            if (isAsync)
            {
                return handler.IsStatic
                    ? $"{typeSymbol}.{handler.Name}(command)"
                    : $"services.GetRequiredService<{typeSymbol}>().{handler.Name}(command)";
            }
            else
            {
                return handler.IsStatic
                    ? $"{typeSymbol}.{handler.Name}(command)"
                    : $"services.GetRequiredService<{typeSymbol}>().{handler.Name}(command)";
            }
        }

        public static string GetQueryInvocation(this IMethodSymbol handler)
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
                    : $"services.GetRequiredService<{handlerClass}>().{handler.Name}(query)";
            }
            else
            {
                return handler.IsStatic
                    ? $"{handlerClass}.{handler.Name}(query)"
                    : $"services.GetRequiredService<{handlerClass}>().{handler.Name}(query)";
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
    }
}
