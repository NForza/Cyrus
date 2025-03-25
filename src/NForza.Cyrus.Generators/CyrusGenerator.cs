using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Generators.Cqrs;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    private CommandProvider CommandProvider { get; } = new();
    private CommandHandlerProvider CommandHandlerProvider { get; } = new();
    private QueryProvider QueryProvider { get; } = new();
    private QueryHandlerProvider QueryHandlerProvider { get; } = new();
    private EventProvider EventProvider { get; } = new();
    private EventHandlerProvider EventHandlerProvider { get; } = new();

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var commandProvider = CommandProvider.GetProvider(context, configProvider);
        var commandHandlerProvider = CommandHandlerProvider.GetProvider(context, configProvider);
        var eventProvider = EventProvider.GetProvider(context, configProvider);
        var eventHandlerProvider = EventHandlerProvider.GetProvider(context, configProvider);
        var queryProvider = QueryProvider.GetProvider(context, configProvider);
        var queryHandlerProvider = QueryHandlerProvider.GetProvider(context, configProvider);

        var cyrusProvider = 
            context.CompilationProvider
            .Combine(commandProvider)
            .Combine(commandHandlerProvider)
            .Combine(queryProvider)
            .Combine(queryHandlerProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var (((((((compilation, commands), commandHandlers), queries), queryHandlers), eventHandlers), events), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(
                    compilation: compilation,
                    commands: commands,
                    commandHandlers: commandHandlers,
                    queries: queries,
                    queryHandlers: queryHandlers,
                    events: events,
                    eventHandlers: eventHandlers,
                    generationConfig: generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
            new CommandHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
            new QueryGenerator().GenerateSource(spc, source, LiquidEngine);
            new QueryHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
            new EventHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
            new EventGenerator().GenerateSource(spc, source, LiquidEngine);
        });
    }
}