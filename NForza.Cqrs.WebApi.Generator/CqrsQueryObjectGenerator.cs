using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
        DebugThisGenerator(false);

        base.Execute(context);

        IEnumerable<string> contractSuffix = Configuration.Contracts;
        var querySuffix = Configuration.Queries.Suffix;
        var methodHandlerName = Configuration.Queries.HandlerName;

        var endpoints = GetAllEndpoints(context.Compilation);
        var ctors = GetConstructorsFromEndpoints(endpoints);
        var expressions = GetExpressionsFromConstructors(ctors);

        foreach (var expression in expressions)
        {
            BuildExpressionList(expression);
        }
    }

    private List<Expression> BuildExpressionList(ExpressionStatementSyntax expression)
    {
        if (expression.Expression is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            Console.WriteLine(memberAccess.Name.Identifier.Text);
        }
        return null;
    }

    private IEnumerable<ExpressionStatementSyntax> GetExpressionsFromConstructors(IEnumerable<IMethodSymbol> ctors)
    {
        return ctors
            .SelectMany(c =>
                {
                    var ctor = c.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax() as ConstructorDeclarationSyntax;

                    if (ctor != null)
                    {
                        var expressionStatements = ctor
                            .DescendantNodes()
                            .OfType<ExpressionStatementSyntax>();
                        return expressionStatements;
                    }
                    return null;
                })
            .Where(expr => expr is not null);
    }

    private IEnumerable<IMethodSymbol> GetConstructorsFromEndpoints(IEnumerable<INamedTypeSymbol> endpoints)
    {
        return endpoints
            .Select(e => e.Constructors.FirstOrDefault(c => c.Parameters.Length == 0))
            .Where(c => c is not null);
    }

    private IEnumerable<INamedTypeSymbol> GetAllEndpoints(Compilation compilation) 
        => GetAllClassesDerivedFrom(compilation, "NForza.Cqrs.WebApi.EndpointGroup");

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