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
        string commandHandlerRegistrations = CreateRegisterCommandHandler(context, commandHandlers);

        var queries = GetAllQueries(context.Compilation, contractSuffix, querySuffix).ToList();
        var queryHandlers = GetAllQueryHandlers(context, queryHandlerMethodName, queries);
        string queryHandlerTypeRegistrations = CreateRegisterTypes(queryHandlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(queryHandlers);
        string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";
        string usings = configuration.EventBus == "MassTransit" ? "using NForza.Cqrs.MassTransit;" : "";

        var replacements = new Dictionary<string, string>
        {
            ["RegisterCommandHandlerTypes"] = commandHandlerTypeRegistrations,
            ["RegisterCommandHandlers"] = commandHandlerRegistrations,
            ["RegisterQueryHandlerTypes"] = queryHandlerTypeRegistrations,
            ["RegisterQueryHandlers"] = queryHandlerRegistrations,
            ["RegisterEventBus"] = eventBusRegistration,
            ["Usings"] = usings
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
            var returnType = handler.ReturnType;

            if (handler.IsStatic)
            {
                source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>((_, query, token) => {typeSymbol}.Query(({queryType})query));");
            }
            else
            {
                source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>((services, query, token) => services.GetRequiredService<{typeSymbol}>().Query(({queryType})query));");

            }
        }
        return source.ToString();
    }

    private static string CreateRegisterTypes(List<IMethodSymbol> handlers)
    {
        var source = new StringBuilder();
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
        services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }
        return source.ToString();
    }

    private static string CreateRegisterCommandHandler(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        INamedTypeSymbol taskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol commandResultSymbol = context.Compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
        var taskOfCommandResultSymbol = taskSymbol.Construct(commandResultSymbol);

        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var commandType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;
            var returnType = handler.ReturnType;
            var isAsync = returnType.Equals(taskOfCommandResultSymbol, SymbolEqualityComparer.Default);

            if (isAsync)
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>((_, command) => {typeSymbol}.Execute(({commandType})command));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>((services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
                }
            }
            else
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>((_, command) => Task.FromResult({typeSymbol}.Execute(({commandType})command)));");
                }
                else
                {
                    source.Append($@"
            handlers.AddHandler<{commandType}>((services, command) => Task.FromResult(services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command)));");
                }
            }
        }
        return source.ToString();
    }
}