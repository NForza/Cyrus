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

    internal static string ForNamedType(ITypeSymbol typeSymbol, LiquidEngine liquidEngine)
    {
        (bool isCollection, ITypeSymbol? elementType) = typeSymbol.IsCollection();
        if (isCollection)
        {
            typeSymbol = elementType;
        }
        string properties = GetPropertiesDeclaration(typeSymbol, liquidEngine);
        string values = GetValuesDeclaration(typeSymbol);
        return $"new ModelTypeDefinition(\"{typeSymbol.Name}\", [{properties}], [{values}], {isCollection.ToString().ToLower()}, {typeSymbol.IsNullable().ToString().ToLower()})";
    }

    internal static string ForQuery(IMethodSymbol queryHandler, LiquidEngine liquidEngine)
    {
        var returnType = queryHandler.GetQueryReturnType();   
        var returnTypeDefinition = ForNamedType(returnType, liquidEngine);
        string queryName = queryHandler.Parameters[0].Type.Name;
        return $"new ModelQueryDefinition(\"{queryName}\", {returnTypeDefinition})";
    }

    private static string GetValuesDeclaration(ITypeSymbol namedType)
    {
        return namedType.TypeKind == TypeKind.Enum ? string.Join(",", namedType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .Select(f => $"\"{f.Name}\"")) : string.Empty;
    }

    private static string GetPropertiesDeclaration(ITypeSymbol typeSymbol, LiquidEngine liquidEngine)
    {
        var propertyDeclarations = typeSymbol.GetPropertyModels().Select(m => liquidEngine.Render(new { property = m }, "model-property"));
        return string.Join(",", propertyDeclarations);
    }
}