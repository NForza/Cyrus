using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Generators.Cqrs;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    private CommandHandlerGenerator CommandHandlerGenerator { get; } = new();
    private QueryGenerator QueryGenerator { get; } = new();
    private QueryHandlerGenerator QueryHandlerGenerator { get; } = new();
    private EventGenerator EventGenerator { get; } = new();
    private EventHandlerGenerator EventHandlerGenerator { get; } = new();

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var commandHandlerProvider = CommandHandlerGenerator.GetProvider(context, configProvider);
        var eventHandlerProvider = EventHandlerGenerator.GetProvider(context, configProvider);
        var eventProvider = EventGenerator.GetProvider(context, configProvider);
        var queryProvider = QueryGenerator.GetProvider(context, configProvider);
        var queryHandlerProvider = QueryHandlerGenerator.GetProvider(context, configProvider);

        var cyrusProvider = 
            context.CompilationProvider
            .Combine(commandHandlerProvider)
            .Combine(queryProvider)
            .Combine(queryHandlerProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var ((((((compilation, commandHandlers), queries), queryHandlers), eventHandlers), events), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(
                    compilation: compilation, 
                    commandHandlers: commandHandlers,
                    queryHandlers: queryHandlers,
                    queries: queries,
                    eventHandlers: eventHandlers,
                    events: events,
                    generationConfig: generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
            CommandHandlerGenerator.GenerateSource(spc, source, LiquidEngine);
            QueryGenerator.GenerateSource(spc, source, LiquidEngine);
            QueryHandlerGenerator.GenerateSource(spc, source, LiquidEngine);
            EventHandlerGenerator.GenerateSource(spc, source, LiquidEngine);
            EventGenerator.GenerateSource(spc, source, LiquidEngine);
        });
    }
}