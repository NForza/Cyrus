using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsQueryObjectGenerator : CqrsSourceGenerator, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
        DebugThisGenerator(true);

        base.Execute(context);

        IEnumerable<string> contractSuffix = Configuration.Contracts;
        var querySuffix = Configuration.Queries.Suffix;
        var methodHandlerName = Configuration.Queries.HandlerName;

        var endpoints = GetAllEndpoints(context.Compilation);
    }

    private object GetAllEndpoints(Compilation compilation)
    {
        var baseTypeSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.WebApi.EndpointGroup");
        var derivedClasses = new List<INamedTypeSymbol>();

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
                        derivedClasses.Add(classSymbol);
                    }
                }
            }
        }

        return derivedClasses;
    }

    private void GenerateQueryProcessorExtensionMethods(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var parameterType = methodSymbol.Parameters[0].Type;
            var returnType = methodSymbol.ReturnType;
            source.Append($@"
    public static {returnType} Query(this IQueryProcessor queryProcessor, {parameterType} command, CancellationToken cancellationToken = default) 
        => queryProcessor.QueryInternal<{parameterType}, {returnType}>(command, cancellationToken);");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("QueryProcessorExtensions.cs", new Dictionary<string, string>
        {
            ["QueryMethods"] = source.ToString()
        });

        context.AddSource($"QueryProcessor.g.cs", resolvedSource.ToString());
    }
}