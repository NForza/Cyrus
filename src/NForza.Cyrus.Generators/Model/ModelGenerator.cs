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
        return $"new ModelTypeDefinition(\"{namedType.Name}\", [{properties}], {namedType.IsCollection().IsMatch.ToString().ToLower()}, {namedType.IsNullable().ToString().ToLower()}, [])";
    }

    private static string GetPropertiesDeclaration(INamedTypeSymbol namedType, LiquidEngine liquidEngine)
    {
        var propertyDeclarations = namedType.GetPropertyModels().Select(m => liquidEngine.Render(new { property = m }, "model-property"));
        return string.Join(",", propertyDeclarations);
    }
}