using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator;

[Generator]
public class CqrsEventConsumerGenerator : GeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeEventHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allEventHandlersProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                return config.Events.Bus == "MassTransit" && IsEventHandler(methodNode, config.Events.HandlerName, config.Events.Suffix);
            })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventHandlersProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventHandlersWithCompilation) =>
        {
            var (queryHandlers, compilation) = eventHandlersWithCompilation;
            if (queryHandlers.Any())
            {
                var sourceText = GenerateEventConsumers(queryHandlers, compilation);
                spc.AddSource($"EventConsumers.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateEventConsumers(ImmutableArray<IMethodSymbol> handlers, Compilation compilation)
    {
        StringBuilder source = new();
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var eventTypeName = methodSymbol.Parameters[0].Type.Name;
            var eventTypeFullName = methodSymbol.Parameters[0].Type.ToFullName();
            source.Append($@"
public class {eventTypeName}Consumer(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory) : EventConsumer<{eventTypeFullName}>(eventHandlerDictionary, serviceScopeFactory)
{{
}}");
        }

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("EventConsumers.cs", new Dictionary<string, string>
        {
            ["EventConsumers"] = source.ToString()
        });

        return resolvedSource;
    }
}