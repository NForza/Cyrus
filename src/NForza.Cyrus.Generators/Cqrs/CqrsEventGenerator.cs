using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Cqrs;

[Generator]
public class CqrsEventGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeEventHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allEventHandlersProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                return IsEventHandler(methodNode, config.Events.HandlerName, config.Events.Suffix);
            })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventHandlersProvider.Combine(context.CompilationProvider).Combine(configProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventHandlersWithCompilation) =>
        {
            var ((queryHandlers, compilation), config) = eventHandlersWithCompilation;
            if (queryHandlers.Any())
            {
                if (config.Events.Bus == "MassTransit")
                {
                    var sourceText = GenerateEventConsumers(queryHandlers);
                    spc.AddSource($"EventConsumers.g.cs", SourceText.From(sourceText, Encoding.UTF8));
                }

                string assemblyName = queryHandlers.First().ContainingAssembly.Name;
                var eventModels = GetPartialModelClass(
                    assemblyName,
                    "Events",
                    "ModelDefinition",
                    queryHandlers.Select(qh => ModelGenerator.For((INamedTypeSymbol)qh.Parameters[0].Type, compilation)));
                spc.AddSource($"model-events.g.cs", SourceText.From(eventModels, Encoding.UTF8));
            }
        });
    }

    private string GenerateEventConsumers(ImmutableArray<IMethodSymbol> handlers)
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