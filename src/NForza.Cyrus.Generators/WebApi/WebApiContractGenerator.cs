using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

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
            var route = contract.GetCommandRoute();
            var propertiesFromRoute = GetRouteProperties(route, command);
            GenerateContract(command, propertiesFromRoute, sourceProductionContext, liquidEngine);
        });
    }

    private static IEnumerable<string> GetRouteProperties(string route, INamedTypeSymbol command)
    {
        var routeProperties = RouteParameterDiscovery.FindAllParametersInRoute(route);
        var publicPropertiesOfCommand = command.GetPublicProperties().Select(p => p.Name).ToList();
        return routeProperties.Where(p => publicPropertiesOfCommand.Contains(p)).ToList();
    }

    public static void GenerateContract(INamedTypeSymbol contract, IEnumerable<string> propertiesFromRoute, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        var constructorArguments = contract.GetConstructorArguments();
        var constructorArgumentNames = constructorArguments.Select(p => p.Name).ToList();
        var publicProperties = contract.GetPublicProperties().Select(p =>
            new
            {
                p.Name,
                Internal = propertiesFromRoute.Contains(p.Name),
                Type = p.Type.ToFullName(),
                IsNullable = p.Type.IsNullable(),
                UseExclamation = UseNullForgivingOperator(p.Type),
                DefaultValue = p.GetDefaultValue(),
                Namespace = p.Type.ContainingNamespace.GetNameOrEmpty()
            })
            .ToList();
        var model = new
        {
            Namespace = contract.ContainingNamespace.GetNameOrEmpty(),
            contract.Name,
            FullName = contract.ToFullName(),
            ConstructorArguments = constructorArguments.Select(p =>
                new
                {
                    p.Name,
                    Type = p.Type.ToFullName(),
                    IsNullable = p.Type.IsNullable(),
                    UseExclamation = UseNullForgivingOperator(p.Type),
                }).ToList(),
            Properties = publicProperties,
            ConstructorProperties = publicProperties.Where(p => constructorArgumentNames.Contains(p.Name)).ToList(),
            Namespaces = publicProperties.Select(p => p.Namespace).Where(ns => ns.Length > 0).Distinct()
        };

        var fileContents = liquidEngine.Render(model, "WebApiContract");

        sourceProductionContext.AddSource($"{contract.Name}Contract.g.cs", fileContents);
    }

    private static bool UseNullForgivingOperator(ITypeSymbol p)
    {
        return p.IsReferenceType || p.SpecialType == SpecialType.System_String;
    }
}