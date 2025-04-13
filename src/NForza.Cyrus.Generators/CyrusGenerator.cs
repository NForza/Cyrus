using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Commands;
using NForza.Cyrus.Generators.Cqrs;
using NForza.Cyrus.Generators.Events;
using NForza.Cyrus.Generators.Generators.Cqrs;
using NForza.Cyrus.Generators.Generators.TypedIds;
using NForza.Cyrus.Generators.Queries;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Generators.WebApi;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigProvider(context);

        var intIdsProvider = new IntIdProvider().GetProvider(context, configProvider);
        var guidIdsProvider = new GuidIdProvider().GetProvider(context, configProvider);
        var stringIdsProvider = new StringIdProvider().GetProvider(context, configProvider);
        var commandProvider = new CommandProvider().GetProvider(context, configProvider);
        var commandHandlerProvider = new CommandHandlerProvider().GetProvider(context, configProvider);
        var eventProvider = new EventProvider().GetProvider(context, configProvider);
        var eventHandlerProvider = new EventHandlerProvider().GetProvider(context, configProvider);
        var queryProvider = new QueryProvider().GetProvider(context, configProvider);
        var queryHandlerProvider = new QueryHandlerProvider().GetProvider(context, configProvider);
        var signalRHubProvider = new SignalRHubProvider().GetProvider(context, configProvider);
        var allCommandsAndHandlersProvider = new AllCommandsAndHandlersProvider().GetProvider(context, configProvider);
        var allQueriesAndHandlersProvider = new AllQueryAndHandlersProvider().GetProvider(context, configProvider);

        var cyrusProvider =
            context.CompilationProvider
            .Combine(intIdsProvider)
            .Combine(guidIdsProvider)
            .Combine(stringIdsProvider)
            .Combine(commandProvider)
            .Combine(commandHandlerProvider)
            .Combine(allCommandsAndHandlersProvider)
            .Combine(queryProvider)
            .Combine(queryHandlerProvider)
            .Combine(allQueriesAndHandlersProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(signalRHubProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var (((((((((((((compilation, intIds), guidIds), stringIds), commands), commandHandlers), allCommandsAndHandlers), queries), queryHandlers), allQueriesAndHandlersProvider), eventHandlers), events), signalRHubs), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(
                    compilation: compilation,
                    guidIds: guidIds,
                    intIds: intIds,
                    stringIds: stringIds,
                    commands: commands,
                    commandHandlers: commandHandlers,
                    allCommandsAndHandlers: allCommandsAndHandlers,
                    queries: queries,
                    queryHandlers: queryHandlers,
                    events: events,
                    eventHandlers: eventHandlers,
                    allQueriesAndHandlers: allQueriesAndHandlersProvider,
                    signalRHubs: signalRHubs,
                    generationConfig: generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
            try
            {
                new StringIdGenerator().GenerateSource(spc, source, LiquidEngine);
                new StringIdTypeConverterGenerator().GenerateSource(spc, source, LiquidEngine);
                new IntIdGenerator().GenerateSource(spc, source, LiquidEngine);
                new IntIdTypeConverterGenerator().GenerateSource(spc, source, LiquidEngine);
                new GuidIdGenerator().GenerateSource(spc, source, LiquidEngine);
                new GuidIdTypeConverterGenerator().GenerateSource(spc, source, LiquidEngine);
                new TypedIdJsonConverterGenerator().GenerateSource(spc, source, LiquidEngine);
                new TypedIdInitializerGenerator().GenerateSource(spc, source, LiquidEngine);
                new CommandHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
                new QueryGenerator().GenerateSource(spc, source, LiquidEngine);
                new QueryHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
                new EventHandlerGenerator().GenerateSource(spc, source, LiquidEngine);
                new EventGenerator().GenerateSource(spc, source, LiquidEngine);
                new SignalRHubGenerator().GenerateSource(spc, source, LiquidEngine);
                new EventHandlerDictionaryGenerator().GenerateSource(spc, source, LiquidEngine);
                new BusRegistrationGenerator().GenerateSource(spc, source, LiquidEngine);
                new WebApiCommandEndpointsGenerator().GenerateSource(spc, source, LiquidEngine);
                new WebApiQueryEndpointsGenerator().GenerateSource(spc, source, LiquidEngine);
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                       DiagnosticDescriptors.InternalCyrusError,
                       Location.None,
                       ex.Message + ": " + ex.StackTrace));
                throw;
            }
        });
    }
}
