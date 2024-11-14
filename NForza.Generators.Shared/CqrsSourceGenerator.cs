using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cqrs.Generator;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

public abstract class CqrsSourceGenerator : GeneratorBase
{
    private IncrementalValueProvider<CqrsConfig>? configuration;
    protected IncrementalValueProvider<CqrsConfig>? Configuration => configuration;

    protected List<IMethodSymbol> GetAllCommandHandlers(GeneratorExecutionContext context, string methodHandlerName, List<INamedTypeSymbol> commands)
    {
        INamedTypeSymbol commandResultSymbol = context.Compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult")!;

        IEnumerable<IMethodSymbol> methodsEndingWithCommand = context.Compilation
                    .GetSymbolsWithName(s => s == methodHandlerName, SymbolFilter.Member)
                    .OfType<IMethodSymbol>()
                    .Where(m => m.Parameters.Length == 1 && commands.Contains(m.Parameters[0].Type, SymbolEqualityComparer.Default))
                    .ToList();
        var asyncCommandHandlers = methodsEndingWithCommand
            .Where(m => ReturnsTaskOfCommandResult(context, m));
        var syncCommandHandlers = methodsEndingWithCommand
            .Where(m => m.ReturnType.Equals(commandResultSymbol, SymbolEqualityComparer.Default));

        var invalidHandlers = methodsEndingWithCommand.Except(asyncCommandHandlers).Except(syncCommandHandlers);
        foreach (var invalidHandler in invalidHandlers)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("CQRS001", "Invalid command handler", $"Command handler {invalidHandler.Name} has invalid signature", "NForza.Cqrs", DiagnosticSeverity.Error, true),
                invalidHandler.Locations.First()));
        }

        return asyncCommandHandlers.Concat(syncCommandHandlers).ToList();
    }

    protected IMethodSymbol? GetMethodSymbolFromContext(GeneratorSyntaxContext context)
    {
        var recordStruct = (MethodDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as IMethodSymbol;
        return symbol;
    }

    protected List<IMethodSymbol> GetAllQueryHandlers(GeneratorExecutionContext context, string methodHandlerName, List<INamedTypeSymbol> queries)
    {
        var symbolsWithCorrectName = context.Compilation.GetSymbolsWithName(s => s == methodHandlerName, SymbolFilter.Member);
        var methodsWithCorrectName = symbolsWithCorrectName.OfType<IMethodSymbol>();
        var methodsWithCorrectNameAndParameters = methodsWithCorrectName
            .Where(m => HasQueryHandlerASignature(m, queries));
        return methodsWithCorrectNameAndParameters.ToList();

        static bool HasQueryHandlerASignature(IMethodSymbol method, List<INamedTypeSymbol> queries)
        {
            return method.Parameters.Length <= 2
                               &&
                               queries.Contains(method.Parameters[0].Type, SymbolEqualityComparer.Default)
                               &&
                               (method.Parameters.Length == 1 || method.Parameters[1].Name == "CancellationToken");
        }
    }

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

    private static bool ReturnsTaskOfCommandResult(GeneratorExecutionContext context, IMethodSymbol handlerType)
    {
        Debug.WriteLine($"Found handler: {handlerType.Name}");

        return IsTaskOfCommandResult(handlerType.ReturnType, context.Compilation);
    }

    private static bool IsTaskOfCommandResult(ITypeSymbol returnType, Compilation compilation)
    {
        INamedTypeSymbol taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType &&
                    namedTypeSymbol.ConstructedFrom.Equals(taskSymbol, SymbolEqualityComparer.Default))
            {

                var genericArguments = namedTypeSymbol.TypeArguments;
                if (genericArguments.Length == 1)
                {
                    var commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
                    return SymbolEqualityComparer.Default.Equals(genericArguments[0], commandResultSymbol);
                }
            }
        }
        return false;
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

    public void GetConfig(IncrementalGeneratorInitializationContext context)
    {
        configuration ??= ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");
    }

    protected bool IsCommandHandler(SyntaxNode syntaxNode)
     => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.Identifier.Text.EndsWith("Execute")
        &&
        methodDeclarationSyntax.ParameterList.Parameters.Count == 1
        &&
        GetTypeName(methodDeclarationSyntax.ParameterList.Parameters[0].Type).EndsWith("Command");

    protected bool IsQueryHandler(SyntaxNode syntaxNode)
    {
        return
            syntaxNode is MethodDeclarationSyntax methodDeclaration
            &&
            methodDeclaration.Identifier.Text.EndsWith("Query")
            &&
            methodDeclaration.ParameterList.Parameters.Count == 1
            &&
            GetTypeName(methodDeclaration.ParameterList.Parameters[0].Type).EndsWith("Query");
    }

    protected bool IsQueryHandler(IMethodSymbol symbol)
        => symbol.Name.EndsWith("Query") && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith("Query");

    protected bool IsCommandHandler(IMethodSymbol symbol) 
        => symbol.Name.EndsWith("Execute") && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith("Command");
}