using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.WebApi;

public static class WebApiContractGenerator
{
    public static void GenerateContracts(IEnumerable<INamedTypeSymbol> contracts, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        contracts.ToList().ForEach(contract => GenerateContract(contract, [], sourceProductionContext, liquidEngine));
    }

    public static void GenerateCommandContracts(IEnumerable<IMethodSymbol> commandHandlers, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        commandHandlers.ToList().ForEach(contract =>
        {
            var command = contract.GetCommandType();
            var route = contract.GetCommandHandlerRoute();
            var propertiesFromRoute = GetRouteProperties(route, command);
            GenerateContract(command, propertiesFromRoute, sourceProductionContext, liquidEngine);
        });
    }

    private static IEnumerable<string> GetRouteProperties(string route, INamedTypeSymbol command)
    {
        var routeProperties = RouteParameterDiscovery.FindAllParametersInRoute(route);
        var publicPropertiesOfCommand = command.GetPublicProperties().Select(p => p.Name).ToList();
        return routeProperties.Select(p => p.Name).Where(p => publicPropertiesOfCommand.Contains(p)).ToList();
    }

    public static void GenerateContract(INamedTypeSymbol contract, IEnumerable<string> propertiesFromRoute, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        if (contract.IsRecordType())
        {
            var constructorArguments = contract.GetConstructorArguments();
            var model = new
            {
                Namespace = contract.ContainingNamespace,
                contract.Name,
                FullName = contract.ToFullName(),
                ConstructorArguments = constructorArguments.Select(p =>
                    new
                    {
                        p.Name,
                        Type = p.Type.ToFullName(),
                        IsNullable = p.Type.IsNullable()
                    }).ToList(),
                Properties = contract.GetPublicProperties().Select(p =>
                    new
                    {
                        p.Name,
                        Internal = propertiesFromRoute.Contains(p.Name),
                        Type = p.Type.ToFullName(),
                        IsNullable = p.Type.IsNullable()
                    })
                    .ToList()
            };

            var fileContents = liquidEngine.Render(model, "WebApiContractRecord");

            sourceProductionContext.AddSource(
               $"{contract.Name}Contract.g.cs",
               SourceText.From(fileContents, Encoding.UTF8));
        }
    }
}