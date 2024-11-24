using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator;

public abstract class CqrsSourceGenerator : GeneratorBase
{
    private IncrementalValueProvider<CqrsConfig>? configuration;
    protected IncrementalValueProvider<CqrsConfig>? Configuration => configuration;

    protected IMethodSymbol? GetMethodSymbolFromContext(GeneratorSyntaxContext context)
    {
        var recordStruct = (MethodDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as IMethodSymbol;
        return symbol;
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

    protected static bool IsDirectlyDerivedFrom(
        INamedTypeSymbol classSymbol,
        string fullyQualifiedBaseClassName)
    {
        var baseType = classSymbol.BaseType;

        return baseType?.ToDisplayString() == fullyQualifiedBaseClassName;
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

    protected bool CouldBeCommandHandler(SyntaxNode syntaxNode)
     => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        &&
        methodDeclarationSyntax.ParameterList.Parameters.Count == 1;

    protected bool CouldBeQueryHandler(SyntaxNode syntaxNode)
    {
        return
            syntaxNode is MethodDeclarationSyntax methodDeclaration
            &&
            methodDeclaration.ParameterList.Parameters.Count == 1;
    }

    protected bool IsQueryHandler(IMethodSymbol symbol, string queryMethodName, string querySuffix)
        => symbol.Name == queryMethodName && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith(querySuffix);

    protected bool IsCommandHandler(IMethodSymbol symbol, string commandHandlerName, string commandSuffix) 
        => symbol.Name.EndsWith(commandHandlerName) && symbol.Parameters.Length == 1 && symbol.Parameters[0].Type.Name.EndsWith(commandSuffix);
}