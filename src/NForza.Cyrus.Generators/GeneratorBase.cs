using System.Collections.Generic;
#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Cqrs.Generator.Config;

namespace NForza.Generators;

public abstract class GeneratorBase
{
    protected TemplateEngine TemplateEngine = new(Assembly.GetExecutingAssembly(), "Templates");
    protected IncrementalValueProvider<GenerationConfig> ConfigFileProvider(IncrementalGeneratorInitializationContext context)
    {
        var configProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Identifier.Text == "CyrusConfiguration",
                transform: (context, _) => GetConfigFromClass((ClassDeclarationSyntax)context.Node))
            .Collect()
            .Select((cfgs, _) => 
            {
                var cfg = cfgs.FirstOrDefault() ?? new GenerationConfig();
                cfg.GenerationType ??= ["domain", "webapi", "contracts"];
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
                        result.Events.Bus = "MassTransit";
                        break;
                    case "GenerateContracts":
                        result.GenerationType = result.GenerationType == null ? ["contracts"] : [.. result.GenerationType, "contracts"];
                        break;
                    case "GenerateDomain":
                        result.GenerationType = result.GenerationType == null ? ["domain"] : [.. result.GenerationType, "domain"];
                        break;
                    case "GenerateWebApi":
                        result.GenerationType = result.GenerationType == null ? ["webapi"] : [.. result.GenerationType, "webapi"];
                        break;
                }
            }
        }
        return result;
    }

    public void DebugThisGenerator(bool debug)
    {
#if DEBUG_ANALYZER
        if (!Debugger.IsAttached && debug)
        {
            Debugger.Launch();
        }
#endif
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
        switch (typeSyntax)
        {
            case IdentifierNameSyntax identifierName:
                return identifierName.Identifier.Text;

            case QualifiedNameSyntax qualifiedName:
                return qualifiedName.Right.Identifier.Text;

            case PredefinedTypeSyntax predefinedType:
                return predefinedType.Keyword.Text;

            default:
                return typeSyntax?.ToString() ?? ""; 
        }
    }

    protected IEnumerable<INamedTypeSymbol> GetAllClassesDerivedFrom(Compilation compilation, string className)
    {
        var baseTypeSymbol = compilation.GetTypeByMetadataName(className);

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var classDeclarations = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                if (classSymbol != null && classSymbol.BaseType != null)
                {
                    if (classSymbol.BaseType.Equals(baseTypeSymbol, SymbolEqualityComparer.Default))
                    {
                        yield return classSymbol;
                    }
                }
            }
        }
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

    static bool IsStruct(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct;
    }

    protected IEnumerable<INamedTypeSymbol> GetAllQueries(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string querySuffix)
    {
        var queriesInDomain = GetAllTypesWithSuffix(compilation, contractProjectSuffixes, querySuffix)
            .ToList();

        return queriesInDomain;
    }

    protected IEnumerable<INamedTypeSymbol> GetAllCommands(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string commandSuffix)
    {
        var commandsInDomain = GetAllTypesWithSuffix(compilation, contractProjectSuffixes, commandSuffix)
            .Where(t => IsStruct(t));

        return commandsInDomain;
    }

    protected bool CouldBeCommandHandler(SyntaxNode syntaxNode)
     => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.ParameterList.Parameters.Count == 1;

    protected bool CouldBeQueryHandler(SyntaxNode syntaxNode) 
        => syntaxNode is MethodDeclarationSyntax methodDeclaration
            &&
            methodDeclaration.ParameterList.Parameters.Count == 1;

    protected bool CouldBeEventHandler(SyntaxNode syntaxNode)
    => syntaxNode is MethodDeclarationSyntax methodDeclaration
        &&
        methodDeclaration.ParameterList.Parameters.Count == 1;

    private IEnumerable<INamedTypeSymbol> GetAllTypesWithSuffix(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string typeSuffix)
    {
        var typesInCurrentCompilation = compilation.GetSymbolsWithName(s => s.EndsWith(typeSuffix), SymbolFilter.Type).OfType<INamedTypeSymbol>();
        foreach (var t in typesInCurrentCompilation)
            yield return t;

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol
                ||
                assemblySymbol.Name.StartsWith("System")
                ||
                assemblySymbol.Name.StartsWith("Microsoft")
                ||
                !contractProjectSuffixes.Any(assemblySymbol.Name.EndsWith))
                continue;

            var allTypes = GetAllTypes(assemblySymbol.GlobalNamespace);

            foreach (var t in allTypes)
            {
                if (t.Name.EndsWith(typeSuffix))
                    yield return t;
            }
        }
    }

    protected static bool IsDirectlyDerivedFrom(
        INamedTypeSymbol classSymbol,
        string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol.BaseType;

        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
    }

    protected bool IsQueryHandler(IMethodSymbol? symbol, string queryMethodName, string querySuffix)
        => symbol != null && symbol.Name == queryMethodName && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith(querySuffix);

    protected bool IsCommandHandler(IMethodSymbol? symbol, string commandHandlerName, string commandSuffix)
        => symbol != null && symbol.Name == commandHandlerName && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith(commandSuffix);

    protected bool IsEventHandler(IMethodSymbol? symbol, string eventHandlerName, string eventSuffix)
        => symbol != null && symbol.Name == eventHandlerName && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith(eventSuffix);
}
