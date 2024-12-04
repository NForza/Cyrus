using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Cqrs.Generator.Config;
using NForza.Generators;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsServiceCollectionGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ParseConfigFile<CyrusConfig>(context, "cyrusConfig.yaml");

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeCommandHandler(syntaxNode) || CouldBeQueryHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allHandlersProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x =>
            {
                var (handler, config) = x;
                return IsCommandHandler(handler, config.Commands.HandlerName, config.Commands.Suffix) || IsQueryHandler(handler, config.Queries.HandlerName, config.Queries.Suffix);
            })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = context.CompilationProvider.Combine(allHandlersProvider).Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (sourceProductionContext, source) =>
        {
            var ((compilation, handlers), config) = source;

            if (config != null && config.GenerationType.Contains("domain"))
            {
                var sourceText = GenerateServiceCollectionExtensions(handlers, config, compilation);
                    sourceProductionContext.AddSource($"ServiceCollection.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateServiceCollectionExtensions(ImmutableArray<IMethodSymbol> handlers, CyrusConfig configuration, Compilation compilation)
    {
        string handlerTypeRegistrations = CreateRegisterTypes(handlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(handlers.Where(x => IsQueryHandler(x, configuration.Queries.HandlerName, configuration.Queries.Suffix)), compilation);
        string commandHandlerRegistrations = CreateRegisterCommandHandler(handlers.Where(x => IsCommandHandler(x, configuration.Commands.HandlerName, configuration.Commands.Suffix)), compilation);
        string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";
        string usings = configuration.EventBus == "MassTransit" ? "using NForza.Cyrus.Cqrs.MassTransit;" : "";

        if (string.IsNullOrEmpty(handlerTypeRegistrations) && string.IsNullOrEmpty(queryHandlerRegistrations) && string.IsNullOrEmpty(commandHandlerRegistrations))
        {
            return string.Empty;
        }

        var replacements = new Dictionary<string, string>
        {
            ["RegisterHandlerTypes"] = handlerTypeRegistrations,
            ["RegisterCommandHandlers"] = commandHandlerRegistrations,
            ["RegisterQueryHandlers"] = queryHandlerRegistrations,
            ["RegisterEventBus"] = eventBusRegistration,
            ["Usings"] = usings
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("CqrsServiceCollectionExtensions.cs", replacements);
        return resolvedSource;
    }

    private string CreateRegisterQueryHandler(IEnumerable<IMethodSymbol> queryHandlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        StringBuilder source = new();
        foreach (var handler in queryHandlers)
        {
            var queryType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;
            var returnType = (INamedTypeSymbol)handler.ReturnType;
            var isAsync = returnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default);
            if (isAsync)
            {
                returnType = returnType.TypeArguments[0] as INamedTypeSymbol;
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>(async (_, query, token) => await {typeSymbol}.Query(({queryType})query));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>(async (services, query, token) => await services.GetRequiredService<{typeSymbol}>().Query(({queryType})query));");

                }

            }
            else
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>((_, query, token) => Task.FromResult<object>({typeSymbol}.Query(({queryType})query)));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnType}>((services, query, token) => Task.FromResult<object>(services.GetRequiredService<{typeSymbol}>().Query(({queryType})query)));");

                }
            }
        }
        return source.ToString();
    }

    private static string CreateRegisterTypes(ImmutableArray<IMethodSymbol> handlers)
    {
        var source = new StringBuilder();
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            if (!typeToRegister.IsStatic)
            {
                source.Append($@"
        services.AddTransient<{typeToRegister.ToDisplayString()}>();");
            }
        }
        return source.ToString();
    }

    private static string CreateRegisterCommandHandler(IEnumerable<IMethodSymbol> handlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol? commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cyrus.Cqrs.CommandResult");

        if (commandResultSymbol == null || taskSymbol == null)
        {
            return string.Empty;
        }

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