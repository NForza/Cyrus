using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsServiceCollectionGenerator : CqrsSourceGenerator, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
        DebugThisGenerator(false);

        var configuration = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        GenerateServiceCollectionExtensions(context, configuration);
    }

    private void GenerateServiceCollectionExtensions(GeneratorExecutionContext context, CqrsConfig configuration)
    {
        IEnumerable<string> contractSuffix = configuration.Contracts;
        var commandSuffix = configuration.Commands.Suffix;
        var commandHandlerMethodName = configuration.Commands.HandlerName;
        var querySuffix = configuration.Queries.Suffix;
        var queryHandlerMethodName = configuration.Queries.HandlerName;

        var commands = GetAllCommands(context.Compilation, contractSuffix, commandSuffix).ToList();
        var commandHandlers = GetAllCommandHandlers(context, commandHandlerMethodName, commands);
        string commandHandlerTypeRegistrations = CreateRegisterTypes(commandHandlers);     
        string commandHandlerRegistrations = CreateRegisterCommandHandler(commandHandlers);

        var queries = GetAllQueries(context.Compilation, contractSuffix, querySuffix).ToList();
        var queryHandlers = GetAllQueryHandlers(context, queryHandlerMethodName, queries);
        string queryHandlerTypeRegistrations = CreateRegisterTypes(queryHandlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(queryHandlers);

        var replacements = new Dictionary<string, string>
        {
            ["RegisterCommandHandlerTypes"] = commandHandlerTypeRegistrations,
            ["RegisterCommandHandlers"] = commandHandlerRegistrations,
            ["RegisterQueryHandlerTypes"] = queryHandlerTypeRegistrations,
            ["RegisterQueryHandlers"] = queryHandlerRegistrations
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("ServiceCollectionExtensions.cs", replacements);
        context.AddSource($"ServiceCollectionExtensions.g.cs", resolvedSource);
    }

    private string CreateRegisterQueryHandler(List<IMethodSymbol> queryHandlers)
    {
        StringBuilder source = new();
        foreach (var handler in queryHandlers)
        {
            var queryType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;

            source.Append($@"
        handlers.AddHandler<{queryType}>((services, command) => services.GetRequiredService<{typeSymbol}>().Query(({queryType})command));");
        }
        return source.ToString();
    }

    private static string CreateRegisterTypes(List<IMethodSymbol> commandHandlers)
    {
        var source = new StringBuilder();
        foreach (var typeToRegister in commandHandlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
            services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }
        return source.ToString();
    }

    private static string CreateRegisterCommandHandler(List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var commandType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;

            source.Append($@"
        handlers.AddHandler<{commandType}>((services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
        }
        return source.ToString();
    }
}