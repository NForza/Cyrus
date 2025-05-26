using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Generators.Roslyn;

public static class ClassDeclarationSyntaxExtensions
{
    public static bool HasAttribute(this ClassDeclarationSyntax classDeclaration, string attributeName)
    {
        return classDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == attributeName);
    }

    public static bool HasAggregateRootAttribute(this ClassDeclarationSyntax classDeclaration) 
        => classDeclaration.HasAttribute("AggregateRoot");

    public static bool HasBaseType(this ClassDeclarationSyntax classDeclaration, string baseTypeName)
    {
        foreach (var baseType in classDeclaration.BaseList?.Types ?? [])
        {
            string typeName = baseType.Type.ToString();

            if (typeName == baseTypeName)
                return true;

            if (typeName.EndsWith($".{baseTypeName}"))
                return true;
        }

        return false;
    }


    public static GenerationConfig GetConfigFromClass(this ClassDeclarationSyntax classDeclarationSyntax)
    {
        var result = new GenerationConfig();

        var constructorSyntax = classDeclarationSyntax.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .Where(constructorDeclarationSyntax => constructorDeclarationSyntax.Body != null)
            .FirstOrDefault();

        if (constructorSyntax != null)
        {
            var methodCalls = constructorSyntax.Body!.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var methodCall in methodCalls)
            {
                var methodName = methodCall.Expression.ToString();

                switch (methodName)
                {
                    case "UseMassTransit":
                        result.EventBus = EventBusType.MassTransit;
                        break;
                    case "GenerateContracts":
                        result.GenerationTarget.Add(GenerationTarget.Contracts);
                        break;
                    case "GenerateDomain":
                        result.GenerationTarget.Add(GenerationTarget.Domain);
                        break;
                    case "GenerateWebApi":
                        result.GenerationTarget.Add(GenerationTarget.WebApi);
                        break;
                }
            }
        }

        if (!result.GenerationTarget.Any())
            result.GenerationTarget.AddRange([GenerationTarget.Domain, GenerationTarget.WebApi, GenerationTarget.Contracts]);
        return result;
    }

}
