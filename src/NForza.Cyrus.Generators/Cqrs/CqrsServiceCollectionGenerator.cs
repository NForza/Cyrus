using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsServiceCollectionGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsCommandHandler() || syntaxNode.IsQueryHandler() || syntaxNode.IsEventHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var allHandlersProvider = incrementalValuesProvider
            .Where(handler => handler != null)
            .Select((x, _) => x!)
            .Collect();

        var assemblyReferences = context.CompilationProvider;

        var typesFromReferencedAssemblyProvider = assemblyReferences
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies()
                    .Where(t => IsQuery(t) || IsCommand(t))
                    .ToList();

                return typesFromAssemblies;
            })
           .Collect();

        var serviceCollectionCombinedProvider = context
            .CompilationProvider
            .Combine(allHandlersProvider)
            .Combine(typesFromReferencedAssemblyProvider)
            .Combine(configProvider)            ;

        context.RegisterSourceOutput(serviceCollectionCombinedProvider, (sourceProductionContext, source) =>
        {
            var (((compilation, handlers), typesFromReferencedAssemblies), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.Domain))
            {
                AddTypeRegistrations(sourceProductionContext, handlers);
                AddQueryHandlerRegistrations(sourceProductionContext, handlers.Where(x => x.IsQueryHandler()), compilation);
                AddCommandHandlerRegistrations(sourceProductionContext, handlers.Where(x => x.IsCommandHandler()), compilation);
//                AddQueryFactoryMethodsRegistrations(sourceProductionContext, typesFromReferencedAssemblies, compilation);
            }
        });
    }

    //private void AddQueryFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, ImmutableArray<INamedTypeSymbol> typesFromReferencedAssemblies, Compilation compilation)
    //{
    //    string queryFactoryRegistrations = GenerateQueryFactoryExtensionMethods(typesFromReferencedAssemblies);

    //    queryFactoryRegistrations = $@"HttpContextObjectFactory x = new HttpContextObjectFactory();
    //         {queryFactoryRegistrations}
    //         services.AddSingleton<IHttpContextObjectFactory>(x);";
    //    var source = LiquidEngine.Render(new { Namespace = "WebApi", Name = "QueryFactoryMethods", Initializer = queryFactoryRegistrations }, "CyrusInitializer");
    //    sourceProductionContext.AddSource($"QueryFactoryMethodRegistrations.g.cs", SourceText.From(source, Encoding.UTF8));
    //}

    private void AddCommandHandlerRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, Compilation compilation)
    {
        string commandHandlerRegistrations = CreateRegisterCommandHandler(handlers, compilation);

        commandHandlerRegistrations = $@"CommandHandlerDictionary handlers = new CommandHandlerDictionary();
             {commandHandlerRegistrations}
             services.AddSingleton(handlers);";

        var source = LiquidEngine.Render(new { Namespace = "Cqrs", Name = "CommandHandlerRegistrations", Initializer = commandHandlerRegistrations }, "CyrusInitializer");
        sourceProductionContext.AddSource($"CommandHandlerRegistrations.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private void AddQueryHandlerRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, Compilation compilation)
    {
        string queryHandlerRegistrations = CreateRegisterQueryHandler(handlers, compilation);
        
        queryHandlerRegistrations = $@"QueryHandlerDictionary handlers = new QueryHandlerDictionary();
             {queryHandlerRegistrations}
             services.AddSingleton(handlers);";

        var source = LiquidEngine.Render(new { Namespace = "Cqrs", Name = "QueryHandlerRegistrations", Initializer = queryHandlerRegistrations }, "CyrusInitializer");
        sourceProductionContext.AddSource($"QueryHandlerRegistrations.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private void AddTypeRegistrations(SourceProductionContext sourceProductionContext, ImmutableArray<IMethodSymbol> handlers)
    {
        string handlerTypeRegistrations = CreateRegisterTypes(handlers);
        var source = LiquidEngine.Render(new { Namespace = "Cqrs", Name = "TypeRegistrations", Initializer = handlerTypeRegistrations }, "CyrusInitializer" );
        sourceProductionContext.AddSource($"TypeRegistrations.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    //private string GenerateServiceCollectionExtensions(ImmutableArray<IMethodSymbol> handlers, ImmutableArray<INamedTypeSymbol> typesFromReferencedAssemblies, GenerationConfig configuration, Compilation compilation)
    //{
    //    string eventHandlerRegistrations = CreateEventHandlerRegistrations(handlers.Where(x => x.IsEventHandler()), compilation);
    //    string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";

    //    if (string.IsNullOrEmpty(handlerTypeRegistrations) && string.IsNullOrEmpty(queryHandlerRegistrations) && string.IsNullOrEmpty(commandHandlerRegistrations))
    //    {
    //        return string.Empty;
    //    }

        

    //    var model = new
    //    {
    //        Imports = configuration.EventBus == "MassTransit" ? ["NForza.Cyrus.MassTransit"] : new List<string>(),
    //        RegisterHandlerTypes = handlerTypeRegistrations,
    //        RegisterCommandHandlers = commandHandlerRegistrations,
    //        RegisterQueryHandlers = queryHandlerRegistrations,
    //        RegisterEventHandlers = eventHandlerRegistrations,
    //        RegisterEventBus = eventBusRegistration,
    //        RegisterQueryFactory = queryFactoryRegistrations
    //    };

    //    var resolvedSource = LiquidEngine.Render(model, "CqrsServiceCollectionExtensions");
    //    return resolvedSource;
    //}

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

    private bool IsQuery(INamedTypeSymbol symbol)
    {
        bool hasQueryAttribute = symbol.IsQuery();
        bool isFrameworkAssembly = symbol.ContainingAssembly.IsFrameworkAssembly();
        return hasQueryAttribute && !isFrameworkAssembly;
    }

    private bool IsCommand(INamedTypeSymbol symbol)
    {
        bool hasCommandAttribute = symbol.IsCommand();
        bool isFrameworkAssembly = symbol.ContainingAssembly.IsFrameworkAssembly();
        return hasCommandAttribute && !isFrameworkAssembly;
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
        foreach (var handler in handlers.Where(h => h.ReturnsCommandResult()))
        {
            var route = handler.Parameters[0].Type.GetCommandRoute();
            var verb = handler.Parameters[0].Type.GetCommandVerb();
            var commandType = handler.Parameters[0].Type.ToFullName();
            var typeSymbol = handler.ContainingType.ToFullName();
            var returnType = handler.ReturnType;
            var handlerName = $"{handler.ContainingType.Name}.{handler.Name}({handler.Parameters[0].Type.Name})";
            var isAsync = returnType.Name == "Task" || returnType.Name == "ValueTask";

            if (isAsync)
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""{route}"", {verb}, (_, command) => {typeSymbol}.Execute(({commandType})command));");
                }
                else
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""{route}"", {verb}, (services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
                }
            }
            else
            {
                if (handler.IsStatic)
                {
                    source.Append($@"
        handlers.AddHandler<{commandType}>(""{route}"", {verb}, (_, command) => Task.FromResult({typeSymbol}.Execute(({commandType})command)));");
                }
                else
                {
                    source.Append($@"
            handlers.AddHandler<{commandType}>(""{route}"", {verb}, (services, command) => Task.FromResult(services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command)));");
                }
            }
        }
        return source.ToString();
    }



    



}