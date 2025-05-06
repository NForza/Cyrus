using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Analyzers;
using NForza.Cyrus.Generators.Commands;
using NForza.Cyrus.Generators.Events;
using NForza.Cyrus.Generators.Generators.Cqrs;
using NForza.Cyrus.Generators.SignalR;
using NForza.Cyrus.Generators.TypedIds;
using NForza.Cyrus.Generators.Validators;
using NForza.Cyrus.Generators.WebApi;

namespace NForza.Cyrus.Generators;

[Generator]
public class CyrusGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var templateOverrides = GetTemplateOverridesProvider(context);

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
        var validatorProvider = new ValidatorProvider().GetProvider(context, configProvider);

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
            .Combine(validatorProvider)
            .Combine(templateOverrides)
            .Combine(configProvider)
            .Select((combinedProviders, _) =>
            {
                var (((((((((((((((compilation, intIds), guidIds), stringIds), commands), commandHandlers), allCommandsAndHandlers), queries), queryHandlers), allQueriesAndHandlers), eventHandlers), events), signalRHubs), validators), templateOverrides), generationConfig) = combinedProviders;
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
                    allQueriesAndHandlers: allQueriesAndHandlers,
                    signalRHubs: signalRHubs,
                    validators: validators,
                    templateOverrides: templateOverrides,
                    generationConfig: generationConfig);
            });

        context.RegisterSourceOutput(cyrusProvider, (spc, source) =>
        {
            try
            {
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(CyrusGeneratorBase)) && !t.IsAbstract)
                    .ToList()
                    .ForEach(t =>
                    {
                        var generator = (CyrusGeneratorBase)Activator.CreateInstance(t);
                        generator.GenerateSource(spc, source, LiquidEngine);
                    });
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
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
            .Select((file, token) => new KeyValuePair<string, string>(file.Path, file.GetText(token)?.ToString() ?? ""))
            .Collect()
            .Select((entries, _) => entries.ToImmutableDictionary(entry => entry.Key, entry => entry.Value));
    }
}
