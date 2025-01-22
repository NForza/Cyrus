﻿using System.Collections.Generic;
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
                predicate: (syntaxNode, _) => IsEvent(syntaxNode),
                transform: (context, _) => GetRecordSymbolFromContext(context));

        var allEventsProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x => x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allEventsProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, eventsWithCompilation) =>
        {
            var (events, compilation) = eventsWithCompilation;
            if (events.Any())
            {
                string assemblyName = events.First().ContainingAssembly.Name;
                var eventModels = GetPartialModelClass(
                    assemblyName,
                    "Events",
                    "ModelTypeDefinition",
                    events.Select(qh => ModelGenerator.ForNamedType(qh, compilation)));
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