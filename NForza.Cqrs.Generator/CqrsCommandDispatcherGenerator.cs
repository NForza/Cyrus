﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsCommandDispatcherGenerator : CqrsSourceGenerator, ISourceGenerator
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
        var commandSuffix = configuration.Commands.Suffix;
        var methodHandlerName = configuration.Commands.HandlerName;

        var commands = GetAllCommands(context.Compilation, contractSuffix, commandSuffix).ToList();
        var handlers = GetAllCommandHandlers(context, methodHandlerName, commands);

        GenerateCommandDispatcherExtensionMethods(context, handlers);
    }

    private void GenerateCommandDispatcherExtensionMethods(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var parameterType = methodSymbol.Parameters[0].Type;
            source.Append($@"
    public static Task<CommandResult> Execute(this ICommandDispatcher dispatcher, {parameterType} command, CancellationToken cancellationToken = default) 
        => dispatcher.ExecuteInternal(command, cancellationToken);");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("CommandDispatcherExtensions.cs", new Dictionary<string, string>
        {
            ["ExecuteMethods"] = source.ToString()
        });

        context.AddSource($"CommandDispatcher.g.cs", resolvedSource.ToString());
    }
}