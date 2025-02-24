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

        var assemblyReferences = context.CompilationProvider;

        var typesFromReferencedAssemblyProvider = assemblyReferences
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.References
                    .Select(ca => compilation.GetAssemblyOrModuleSymbol(ca) as IAssemblySymbol)
                    .SelectMany(ass => ass != null ? GetAllTypesRecursively(ass.GlobalNamespace) : [])
                    .Where(t => IsQuery(t) || IsCommand(t))
                    .ToList();

                var csharpCompilation = (CSharpCompilation)compilation;
                var typesInCompilation = GetAllTypesRecursively(csharpCompilation.GlobalNamespace)
                    .Where(IsQuery)
                    .ToList();

                return typesFromAssemblies.Union(typesInCompilation, SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>();
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
                var sourceText = GenerateServiceCollectionExtensions(handlers, typesFromReferencedAssemblies, config, compilation);
                sourceProductionContext.AddSource($"ServiceCollection.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateServiceCollectionExtensions(ImmutableArray<IMethodSymbol> handlers, ImmutableArray<INamedTypeSymbol> typesFromReferencedAssemblies, GenerationConfig configuration, Compilation compilation)
    {
        string handlerTypeRegistrations = CreateRegisterTypes(handlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(handlers.Where(x => x.IsQueryHandler()), compilation);
        string commandHandlerRegistrations = CreateRegisterCommandHandler(handlers.Where(x => x.IsCommandHandler()), compilation);
        string eventHandlerRegistrations = CreateEventHandlerRegistrations(handlers.Where(x => x.IsEventHandler()), compilation);
        string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";
        string queryFactoryRegistrations = GenerateQueryFactoryExtensionMethods(typesFromReferencedAssemblies);

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
            RegisterQueryFactory = queryFactoryRegistrations
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
        foreach (var handler in handlers)
        {
            var route = handler.Parameters[0].Type.GetCommandRoute();
            var verb = handler.Parameters[0].Type.GetCommandVerb();
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

    private static string[] assembliesToSkip = ["System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle", "RabbitMQ", "MassTransit"];
    private IEnumerable<INamedTypeSymbol> GetAllTypesRecursively(INamespaceSymbol namespaceSymbol)
    {
        var assemblyName = namespaceSymbol?.ContainingAssembly?.Name;
        if (assemblyName != null && assembliesToSkip.Any(n => assemblyName.StartsWith(n)))
        {
            return [];
        }

        var types = namespaceSymbol!.GetTypeMembers();
        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            types = types.AddRange(GetAllTypesRecursively(subNamespace));
        }
        return types;
    }

    private string GenerateQueryFactoryExtensionMethods(ImmutableArray<INamedTypeSymbol> queries)
    {
        StringBuilder source = new();
        foreach (var query in queries)
        {
            var queryTypeName = query.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            source.Append($@"    x.Register<{queryTypeName}>(ctx => {GetConstructionExpression(query)}");
        }

        return source.ToString();
    }

    private string GetConstructionExpression(INamedTypeSymbol query)
    {
        static IEnumerable<IPropertySymbol> GetPublicProperties(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        }

        var queryTypeName = query.ToFullName();
        var ctor = new StringBuilder(@$"new {queryTypeName}");
        var constructorProperties = GenerateConstructorParameters(query, ctor);
        var propertiesToInitialize = GetPublicProperties(query).Where(p => !constructorProperties.Contains(p.Name)).ToList();
        if (propertiesToInitialize.Count > 0)
        {
            ctor.Append("{");
            var propertyInitializer = new List<string>();
            foreach (var prop in propertiesToInitialize)
            {
                propertyInitializer.Add(@$"{prop.Name} = ({prop.Type.ToFullName()})x.GetPropertyValue(""{prop.Name}"", ctx, typeof({prop.Type}))");
            }
            ctor.Append(string.Join(", ", propertyInitializer));
            ctor.Append("}");
        }
        ctor.AppendLine(");");
        return ctor.ToString();
    }

    private List<string> GenerateConstructorParameters(INamedTypeSymbol query, StringBuilder ctor)
    {
        var constructorWithLeastParameters = query.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderBy(c => c.Parameters.Length)
                .FirstOrDefault();
        if (constructorWithLeastParameters == null)
        {
            return [];
        }
        ctor.Append("(");

        var result = new List<string>();
        var firstParam = constructorWithLeastParameters.Parameters.FirstOrDefault();
        foreach (var param in constructorWithLeastParameters.Parameters)
        {
            if (!param.Equals(firstParam, SymbolEqualityComparer.Default))
            {
                ctor.Append(", ");
            }
            ctor.Append($"({param.Type.ToFullName()})x.GetPropertyValue(\"{param.Name}\", ctx, typeof({param.Type.ToFullName()}))");
            result.Add(param.Name);
        }
        ctor.Append(")");
        return result;
    }

}