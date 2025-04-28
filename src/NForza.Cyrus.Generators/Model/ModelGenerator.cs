using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Model;

internal class ModelGenerator
{
    internal static string ForHub(SignalRHubClassDefinition signalRHub, LiquidEngine liquidEngine)
    {
        var model = new
        {
            signalRHub.Name,
            signalRHub.Path,
            signalRHub.Commands,
            signalRHub.Queries,
            signalRHub.Events
        };

        return liquidEngine.Render(model, "model-hub");
    }

    internal static string ForNamedType(INamedTypeSymbol namedType, LiquidEngine liquidEngine)
    {
        string properties = GetPropertiesDeclaration(namedType, liquidEngine);
        string values = GetValuesDeclaration(namedType);
        return $"new ModelTypeDefinition(\"{namedType.Name}\", [{properties}], [{values}], {namedType.IsCollection().IsMatch.ToString().ToLower()}, {namedType.IsNullable().ToString().ToLower()})";
    }

    internal static string ForQuery(IMethodSymbol queryHandler, LiquidEngine liquidEngine)
    {
        var returnType = (INamedTypeSymbol) queryHandler.GetQueryReturnType();   
        var returnTypeDefinition = ForNamedType(returnType, liquidEngine);
        string queryName = queryHandler.Parameters[0].Type.Name;
        return $"new ModelQueryDefinition(\"{queryName}\", {returnTypeDefinition})";
    }

    private static string GetValuesDeclaration(INamedTypeSymbol namedType)
    {
        return namedType.TypeKind == TypeKind.Enum ? string.Join(",", namedType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .Select(f => $"\"{f.Name}\"")) : string.Empty;
    }

    private static string GetPropertiesDeclaration(INamedTypeSymbol namedType, LiquidEngine liquidEngine)
    {
        var propertyDeclarations = namedType.GetPropertyModels().Select(m => liquidEngine.Render(new { property = m }, "model-property"));
        return string.Join(",", propertyDeclarations);
    }
}