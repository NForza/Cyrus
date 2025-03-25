using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Generators.Cqrs;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    private CommandHandlerGenerator CommandHandlerGenerator { get; } = new();
    private EventGenerator EventGenerator { get; } = new();
    private EventHandlerGenerator EventHandlerGenerator { get; } = new();

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var configProvider = ConfigProvider(context);

        var commandHandlerProvider = CommandHandlerGenerator.GetProvider(context, configProvider);
        var eventHandlerProvider = EventHandlerGenerator.GetProvider(context, configProvider);
        var eventProvider = EventGenerator.GetProvider(context, configProvider);

        var cyrusProvider = 
            context.CompilationProvider
            .Combine(commandHandlerProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var ((((compilation, commandHandlers), eventHandlers), events), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(
                    compilation: compilation, 
                    commandHandlers: commandHandlers, 
                    eventHandlers: eventHandlers,
                    events: events,
                    generationConfig: generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
            CommandHandlerGenerator.GenerateSource(spc, source, LiquidEngine);
            EventHandlerGenerator.GenerateSource(spc, source, LiquidEngine);
            EventGenerator.GenerateSource(spc, source, LiquidEngine);
        });
    }
}