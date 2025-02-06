using System.Collections.Generic;
#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

public abstract class CyrusGeneratorBase : IncrementalGeneratorBase
{
    private LiquidEngine? liquidEngine = null;  
    protected LiquidEngine LiquidEngine 
    { 
        get
        {
            liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly());
            return liquidEngine;
        }
    } 

    protected IncrementalValueProvider<GenerationConfig> ConfigProvider(IncrementalGeneratorInitializationContext context)
    {
        var configProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Identifier.Text == "CyrusConfiguration",
                transform: (context, _) => GetConfigFromClass((ClassDeclarationSyntax)context.Node))
            .Collect()
            .Select((cfgs, _) =>
            {
                var cfg = cfgs.FirstOrDefault() ?? new GenerationConfig();
                if (!cfg.GenerationTarget.Any())
                    cfg.GenerationTarget.AddRange([GenerationTarget.Domain, GenerationTarget.WebApi, GenerationTarget.Contracts]);
                return cfg;
            });
        return configProvider;
    }

    private GenerationConfig GetConfigFromClass(ClassDeclarationSyntax classDeclarationSyntax)
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
                        result.EventBus = "MassTransit";
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
                    case "UseContractsFromAssembliesContaining":
                        result.Contracts =
                                [.. methodCall.ArgumentList.Arguments
                                    .Select(argument => argument.Expression as LiteralExpressionSyntax)
                                    .Where(literal => literal != null)
                                    .Select(literal => literal!.Token.ValueText)];
                        break;
                }
            }
        }
        return result;
    }

    protected IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var nestedType in GetAllTypes(nestedNamespace))
                {
                    yield return nestedType;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                yield return namedType;
            }
        }
    }

    public virtual void Initialize(IncrementalGeneratorInitializationContext context) { }

    protected string GetTypeName(TypeSyntax? typeSyntax)
    {
        return typeSyntax switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.Text,
            _ => typeSyntax?.ToString() ?? "",
        };
    }

    protected IMethodSymbol? GetMethodSymbolFromContext(GeneratorSyntaxContext context)
    {
        var recordStruct = (MethodDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as IMethodSymbol;
        return symbol;
    }

    protected INamedTypeSymbol? GetClassSymbolFromContext(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        return symbol;
    }

    protected INamedTypeSymbol? GetRecordSymbolFromContext(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        return symbol;
    }


    protected static bool IsDirectlyDerivedFrom(
        INamedTypeSymbol classSymbol,
        string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol.BaseType;

        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
    }

    public string GetPartialModelClass(string assemblyName, string propertyName, string propertyType, IEnumerable<string> propertyValues)
    {
        var model = new 
        {
            AssemblyName = assemblyName,
            PropertyName = propertyName,
            PropertyType = propertyType,
            Properties = string.Join(",", propertyValues)
        };
        var source = LiquidEngine.Render(model, "CyrusModel");
        return source;
    }
}
