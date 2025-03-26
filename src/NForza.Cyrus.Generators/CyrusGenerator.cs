using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Commands;
using NForza.Cyrus.Generators.Cqrs;
using NForza.Cyrus.Generators.Events;
using NForza.Cyrus.Generators.Generators.Cqrs;
using NForza.Cyrus.Generators.Generators.TypedIds;
using NForza.Cyrus.Generators.Generators.WebApi;
using NForza.Cyrus.Generators.Queries;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

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

        var cyrusProvider = 
            context.CompilationProvider
            .Combine(intIdsProvider)
            .Combine(guidIdsProvider)
            .Combine(stringIdsProvider)
            .Combine(commandProvider)
            .Combine(commandHandlerProvider)
            .Combine(queryProvider)
            .Combine(queryHandlerProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var ((((((((((compilation, intIds), guidIds), stringIds), commands), commandHandlers), queries), queryHandlers), eventHandlers), events), generationConfig) = combinedProviders;
                return new CyrusGenerationContext(
                    compilation: compilation,
                    guidIds: guidIds,
                    intIds: intIds,
                    stringIds: stringIds,
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
                    new BusRegistrationGenerator().GenerateSource(spc, source, LiquidEngine);   
                }
                catch (Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CY0001",
                            "Cyrus Generator Error",
                            $"{ex.Message}\n{ex.StackTrace}",
                            "CyrusGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            Location.None));
                    throw;
                }
            });
    }        
}
