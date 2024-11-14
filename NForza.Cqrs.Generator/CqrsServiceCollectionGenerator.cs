﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cqrs.Generator.Config;
using NForza.Generators;

namespace NForza.Cqrs.Generator;

[Generator]
public class CqrsServiceCollectionGenerator : CqrsSourceGenerator, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ParseConfigFile<CqrsConfig>(context, "cqrsConfig.yaml");

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => IsCommandHandler(syntaxNode) || IsQueryHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allHandlersProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        var combinedProvider = context.CompilationProvider.Combine(allHandlersProvider.Combine(configProvider));

        context.RegisterSourceOutput(combinedProvider, (sourceProductionContext, source) =>
        {
            var (compilation, (handlers, config)) = source;
            var sourceText = GenerateServiceCollectionExtensions(handlers, config, compilation);
            sourceProductionContext.AddSource($"ServiceCollection.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        });
    }

    private string GenerateServiceCollectionExtensions(ImmutableArray<IMethodSymbol> handlers, CqrsConfig configuration, Compilation compilation)
    {
        string handlerTypeRegistrations = CreateRegisterTypes(handlers);
        string queryHandlerRegistrations = CreateRegisterQueryHandler(handlers.Where(IsQueryHandler));
        string commandHandlerRegistrations = CreateRegisterCommandHandler(handlers.Where(IsCommandHandler), compilation);
        string eventBusRegistration = $@"services.AddSingleton<IEventBus, {configuration.EventBus}EventBus>();";
        string usings = configuration.EventBus == "MassTransit" ? "using NForza.Cqrs.MassTransit;" : "";

        var replacements = new Dictionary<string, string>
        {
            ["RegisterHandlerTypes"] = handlerTypeRegistrations,
            ["RegisterCommandHandlers"] = commandHandlerRegistrations,
            ["RegisterQueryHandlers"] = queryHandlerRegistrations,
            ["RegisterEventBus"] = eventBusRegistration,
            ["Usings"] = usings
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("ServiceCollectionExtensions.cs", replacements);
        return resolvedSource;
    }

    private string CreateRegisterQueryHandler(IEnumerable<IMethodSymbol> queryHandlers)
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

    private static string CreateRegisterTypes(ImmutableArray<IMethodSymbol> handlers)
    {
        var source = new StringBuilder();
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
        services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }
        return source.ToString();
    }

    private static string CreateRegisterCommandHandler(IEnumerable<IMethodSymbol> handlers, Compilation compilation)
    {
        INamedTypeSymbol taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
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