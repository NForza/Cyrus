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

        return liquidEngine.Render(model, "Model-hub");
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
        return $"new ModelTypeDefinition(\"{typeSymbol.Name}\", \"{typeSymbol.ToAssemblyQualifiedName()}\", \"{typeSymbol.Description()}\", [{properties}], [{values}], {isCollection.ToString().ToLower()}, {typeSymbol.IsNullable().ToString().ToLower()})";
    }

    internal static string ForQueryHandler(IMethodSymbol queryHandler, LiquidEngine liquidEngine)
    {
        var queryModelDefinition = ForNamedType(queryHandler.Parameters[0].Type, liquidEngine);
        var returnType = queryHandler.GetQueryReturnType();
        var returnTypeDefinition = returnType.Name == "Stream" ? ForStream(returnType.IsNullable()) : ForNamedType(returnType, liquidEngine);
        string queryName = queryHandler.Parameters[0].Type.Name;
        return $"new ModelQueryDefinition({queryModelDefinition}, {returnTypeDefinition})";
    }

    private static string ForStream(bool isNullable) 
        => $"new ModelTypeDefinition(\"byte\", \"System.Byte\", string.Empty, [], [], true, {isNullable.ToString().ToLower()})";

    private static string GetValuesDeclaration(ITypeSymbol namedType)
    {
        return namedType.TypeKind == TypeKind.Enum ? string.Join(",", namedType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .Select(f => $"\"{f.Name}\"")) : string.Empty;
    }

    private static string GetPropertiesDeclaration(ITypeSymbol typeSymbol, LiquidEngine liquidEngine)
    {
        var propertyDeclarations = typeSymbol.GetPropertyModels().Select(m => liquidEngine.Render(new { property = m }, "Model-property"));
        return string.Join(",", propertyDeclarations);
    }

    internal static string ForCommandEndpoint((INamedTypeSymbol NamedTypeSymbol, string HttpVerb, string Route) em, LiquidEngine liquidEngine)
    {
        return $"new ModelEndpointDefinition(HttpVerb.{em.HttpVerb}, \"{em.Route}\", \"{em.NamedTypeSymbol.Name}\", \"\")";
    }

    internal static string ForQueryEndpoint((INamedTypeSymbol NamedTypeSymbol, string HttpVerb, string Route) em, LiquidEngine liquidEngine)
    {
        return $"new ModelEndpointDefinition(HttpVerb.{em.HttpVerb}, \"{em.Route}\", \"\", \"{em.NamedTypeSymbol.Name}\")";
    }

}