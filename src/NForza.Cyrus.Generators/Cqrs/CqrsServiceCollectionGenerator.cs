using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsServiceCollectionGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsCommandHandler() || syntaxNode.IsQueryHandler() || syntaxNode.IsEventHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var allHandlersProvider = incrementalValuesProvider
            .Where(handler => handler != null)
            .Select((x, _) => x!)
            .Collect();

        var combinedProvider = context.CompilationProvider.Combine(allHandlersProvider).Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (sourceProductionContext, source) =>
        {
            var ((compilation, handlers), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.Domain))
            {
                var sourceText = GenerateServiceCollectionExtensions(handlers, config, compilation);
                sourceProductionContext.AddSource($"ServiceCollection.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateServiceCollectionExtensions(ImmutableArray<IMethodSymbol> handlers, GenerationConfig configuration, Compilation compilation)
    {
        string handlerTypeRegistrations = CreateRegisterTypes(handlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(handlers.Where(x => x.IsQueryHandler()), compilation);
        string commandHandlerRegistrations = CreateRegisterCommandHandler(handlers.Where(x => x.IsCommandHandler()), compilation);
        string eventHandlerRegistrations = CreateEventHandlerRegistrations(handlers.Where(x => x.IsEventHandler()), compilation); 
        string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";

        if (string.IsNullOrEmpty(handlerTypeRegistrations) && string.IsNullOrEmpty(queryHandlerRegistrations) && string.IsNullOrEmpty(commandHandlerRegistrations))
        {
            return string.Empty;
        }

        var model = new
        {
            Imports = configuration.EventBus == "MassTransit" ? ["NForza.Cyrus.MassTransit"] : new List<string>(),
            RegisterHandlerTypes = handlerTypeRegistrations,
            RegisterCommandHandlers = commandHandlerRegistrations,
            RegisterQueryHandlers = queryHandlerRegistrations,
            RegisterEventHandlers = eventHandlerRegistrations,
            RegisterEventBus = eventBusRegistration,
        };

        var resolvedSource = LiquidEngine.Render(model, "CqrsServiceCollectionExtensions");
        return resolvedSource;
    }

    private string CreateEventHandlerRegistrations(IEnumerable<IMethodSymbol> eventHandlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        StringBuilder source = new();
        foreach (var eventHandler in eventHandlers)
        {
            var eventType = eventHandler.Parameters[0].Type.ToFullName();
            var typeSymbol = eventHandler.ContainingType;
            var typeSymbolName = typeSymbol.ToFullName();
            var handlerName = $"{typeSymbol.Name}.{eventHandler.Name}({eventHandler.Parameters[0].Type.Name})";
            var returnType = (INamedTypeSymbol)eventHandler.ReturnType;
            var isAsync = returnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default);
            if (isAsync)
            {
                if (eventHandler.IsStatic)
                {
                    source.Append($@"
        handlers.AddEventHandler<{eventType}>(""static async {handlerName}"", (_, @event) => {typeSymbolName}.Handle(({eventType})@event));");
                }
                else
                {
                    source.Append($@"
        handlers.AddEventHandler<{eventType}>(""async {handlerName}"", (services, @event) => services.GetRequiredService<{typeSymbolName}>().Handle(({eventType})@event));");
                }

            }
            else
            {
                if (eventHandler.IsStatic)
                {
                    source.Append($@"
        handlers.AddEventHandler<{eventType}>(""static {handlerName}"", (_, @event) => {typeSymbolName}.Handle(({eventType})@event));");
                }
                else
                {
                    source.Append($@"
        handlers.AddEventHandler<{eventType}>(""{handlerName}"", (services, @event) => services.GetRequiredService<{typeSymbolName}>().Handle(({eventType})@event));");
                }
            }
        }
        return source.ToString();
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
            var route = handler.Parameters[0].Type.GetQueryRoute();
            var queryType = handler.Parameters[0].Type.ToFullName();
            var typeSymbol = handler.ContainingType;
            var returnType = (INamedTypeSymbol)handler.ReturnType;
            var returnTypeFullName = returnType?.ToFullName();
            var handlerName = $"{typeSymbol.Name}.{handler.Name}({handler.Parameters[0].Type.Name})";
            var isAsync = returnType?.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default) ?? false;
            if (isAsync)
            {
                var realReturnType = returnType!.TypeArguments[0];
                returnTypeFullName = realReturnType?.ToFullName();
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnTypeFullName}>(""{route}"", async (_, query, token) => await {typeSymbol}.Query(({queryType})query));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnTypeFullName}>(""{route}"", async (services, query, token) => await services.GetRequiredService<{typeSymbol}>().Query(({queryType})query));");
                }
            }
            else
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnTypeFullName}>(""{route}"", (_, query, token) => Task.FromResult<object>({typeSymbol}.Query(({queryType})query)));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{queryType}, {returnTypeFullName}>(""{route}"", (services, query, token) => Task.FromResult<object>(services.GetRequiredService<{typeSymbol}>().Query(({queryType})query)));");
                }
            }
        }
        return source.ToString();
    }

    private static string CreateRegisterTypes(ImmutableArray<IMethodSymbol> handlers)
    {
        var source = new StringBuilder();
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default).Where(h => h != null))
        {
            if (!typeToRegister!.IsStatic)
            {
                source.Append($@"
        services.AddScoped<{typeToRegister.ToFullName()}>();");
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
            var commandType = handler.Parameters[0].Type.ToFullName();
            var typeSymbol = handler.ContainingType.ToFullName();
            var returnType = handler.ReturnType;
            var handlerName = $"{handler.ContainingType.Name}.{handler.Name}({handler.Parameters[0].Type.Name})";
            var isAsync = returnType.Equals(taskOfCommandResultSymbol, SymbolEqualityComparer.Default);

            if (isAsync)
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""static async {handlerName}"", (_, command) => {typeSymbol}.Execute(({commandType})command));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""async {handlerName}"", (services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
                }
            }
            else
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""static {handlerName}"", (_, command) => Task.FromResult({typeSymbol}.Execute(({commandType})command)));");
                }
                else
                {
                    source.Append($@"
            handlers.AddHandler<{commandType}>(""{handlerName}"", (services, command) => Task.FromResult(services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command)));");
                }
            }
        }
        return source.ToString();
    }
}