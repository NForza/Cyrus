using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsQueryHandlerGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => IsQueryHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allQueryHandlersProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(allQueryHandlersProvider, (spc, queryHandlers) =>
        {
            var sourceText = GenerateQueryProcessorExtensionMethods(queryHandlers);
            spc.AddSource($"QueryProcessor.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        });
    }

    private string GenerateQueryProcessorExtensionMethods( ImmutableArray<IMethodSymbol> handlers)
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

        return resolvedSource;
    }
}