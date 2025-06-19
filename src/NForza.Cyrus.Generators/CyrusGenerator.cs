using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Aggregates;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Commands;
using NForza.Cyrus.Generators.Events;
using NForza.Cyrus.Generators.Queries;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Generators.ValueTypes;
using NForza.Cyrus.Generators.Validators;
using NForza.Cyrus.Generators.WebApi;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var isTestProjectProvider = context.CodeGenerationSuppressed();

        var templateOverrides = GetTemplateOverridesProvider(context);

        var configProvider = ConfigProvider(context);

        var intValuesProvider = new IntValueProvider().GetProvider(context, configProvider);
        var doubleValuesProvider = new DoubleValueProvider().GetProvider(context, configProvider);
        var guidValuesProvider = new GuidValueProvider().GetProvider(context, configProvider);
        var stringValuesProvider = new StringValueProvider().GetProvider(context, configProvider);
        var commandProvider = new CommandProvider().GetProvider(context, configProvider);
        var commandHandlerProvider = new CommandHandlerProvider().GetProvider(context, configProvider);
        var eventProvider = new EventProvider().GetProvider(context, configProvider);
        var eventHandlerProvider = new EventHandlerProvider().GetProvider(context, configProvider);
        var queryProvider = new QueryProvider().GetProvider(context, configProvider);
        var queryHandlerProvider = new QueryHandlerProvider().GetProvider(context, configProvider);
        var signalRHubProvider = new SignalRHubProvider().GetProvider(context, configProvider);
        var allCommandsAndHandlersProvider = new AllCommandsAndHandlersProvider().GetProvider(context, configProvider);
        var allQueriesAndHandlersProvider = new AllQueryAndHandlersProvider().GetProvider(context, configProvider);
        var allEventsProvider = new AllEventsProvider().GetProvider(context, configProvider);
        var validatorProvider = new ValidatorProvider().GetProvider(context, configProvider);
        var aggregateRootProvider = new AggregateRootProvider().GetProvider(context, configProvider);

        var cyrusGenerationContext =
            context.CompilationProvider
            .Combine(intValuesProvider)
            .Combine(doubleValuesProvider)
            .Combine(guidValuesProvider)
            .Combine(stringValuesProvider)
            .Combine(commandProvider)
            .Combine(commandHandlerProvider)
            .Combine(allCommandsAndHandlersProvider)
            .Combine(queryProvider)
            .Combine(queryHandlerProvider)
            .Combine(allQueriesAndHandlersProvider)
            .Combine(eventHandlerProvider)
            .Combine(eventProvider)
            .Combine(allEventsProvider)
            .Combine(aggregateRootProvider)
            .Combine(signalRHubProvider)
            .Combine(validatorProvider)
            .Combine(templateOverrides)
            .Combine(configProvider)
            .Combine(isTestProjectProvider)
            .Select((combinedProviders, _) =>
            {
                var (((((((((((((((((((compilation, intValues), doubleValues), guidValues), StringValues), commands), commandHandlers), allCommandsAndHandlers), queries), queryHandlers), allQueriesAndHandlers), eventHandlers), events), allEvents), aggregateRoots), signalRHubs), validators), templateOverrides), generationConfig), isTestProject) = combinedProviders;
                var liquidEngine = new LiquidEngine(Assembly.GetExecutingAssembly(), new(templateOverrides));
                return new CyrusGenerationContext(
                    compilation: compilation,
                    guidValues: guidValues,
                    intValues: intValues,
                    doubleValues: doubleValues,
                    stringValues: StringValues,
                    commands: commands,
                    commandHandlers: commandHandlers,
                    allCommandsAndHandlers: allCommandsAndHandlers,
                    queries: queries,
                    queryHandlers: queryHandlers,
                    events: events,
                    allEvents: allEvents,
                    eventHandlers: eventHandlers,
                    allQueriesAndHandlers: allQueriesAndHandlers,
                    aggregateRoots: aggregateRoots,
                    signalRHubs: signalRHubs,
                    validators: validators,
                    generationConfig: generationConfig,
                    liquidEngine: liquidEngine,
                    isTestProject: isTestProject);
            });

        context.RegisterSourceOutput(cyrusGenerationContext, (sourceProductionContext, cyrusGenerationContext) =>
        {
            try
            {
                if (cyrusGenerationContext.IsTestProject)
                    return;

                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(CyrusGeneratorBase)) && !t.IsAbstract)
                    .ToList()
                    .ForEach(t =>
                    {
                        var generator = (CyrusGeneratorBase)Activator.CreateInstance(t);
                        generator.GenerateSource(sourceProductionContext, cyrusGenerationContext);
                    });
            }
            catch (Exception ex)
            {
                sourceProductionContext.ReportDiagnostic(Diagnostic.Create(
                       DiagnosticDescriptors.InternalCyrusError,
                       Location.None,
                       ex.Message + ": " + ex.StackTrace.Replace("\r", "").Replace("\n", ",")));
                throw;
            }
        });
    }

    private IncrementalValueProvider<ImmutableDictionary<string, string>> GetTemplateOverridesProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase))
            .Select((file, token) => new KeyValuePair<string, string>(Path.GetFileName(file.Path), file.GetText(token)?.ToString() ?? ""))
            .Collect()
            .Select((entries, _) => entries.ToImmutableDictionary(entry => entry.Key, entry => entry.Value));
    }
}
