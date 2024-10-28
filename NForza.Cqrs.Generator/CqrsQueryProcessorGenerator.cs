using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsQueryHandlerGenerator : CqrsSourceGenerator, ISourceGenerator
{    
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER 
        if (!Debugger.IsAttached && false)
        {
            Debugger.Launch();
        }
#endif
        var configuration = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        IEnumerable<string> contractSuffix = configuration.Contracts;
        var querySuffix = configuration.Commands.Suffix;
        var methodHandlerName = configuration.Commands.HandlerName;

        var commands = GetAllQueries(context.Compilation, contractSuffix, querySuffix).ToList();
        var handlers = GetAllQueryHandlers(context, methodHandlerName, commands);

        GenerateQueryProcessorExtensionMethods(context, handlers);
    }

    private void GenerateQueryProcessorExtensionMethods(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var parameterType = methodSymbol.Parameters[0].Type;
            source.Append($@"
    public static Task<CommandResult> Execute(this IQueryProcessor queryProcessor, {parameterType} command, CancellationToken cancellationToken = default) 
        => queryProcessor.QueryInternal(command, cancellationToken);");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("QueryProcessorExtensions.cs", new Dictionary<string, string>
        {
            ["QueryMethods"] = source.ToString()
        });

        context.AddSource($"QueryProcessor.g.cs", resolvedSource.ToString());
    }
}