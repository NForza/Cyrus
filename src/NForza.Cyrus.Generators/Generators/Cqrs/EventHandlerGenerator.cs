using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.Cqrs;

public class EventHandlerGenerator : CyrusGeneratorBase<ImmutableArray<IMethodSymbol>>
{
    public override IncrementalValueProvider<ImmutableArray<IMethodSymbol>> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider)
    {
        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode.IsEventHandler(),
                transform: (context, _) => context.GetMethodSymbolFromContext());

        var allEventHandlersProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();
        return allEventHandlersProvider;
    }

    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var eventHandlers = cyrusProvider.EventHandlers;
        if (eventHandlers.Any())
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var eventHandlerRegistrations = string.Join(Environment.NewLine, eventHandlers
                .Select(x => x.ContainingType)
                .Where(x => x != null)
                .Distinct(SymbolEqualityComparer.Default)
                .Select(eht => $" services.AddTransient<{eht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            var ctx = new
            {
                Usings = new string[] { "NForza.Cyrus.Cqrs" },
                Namespace = "EventHandlers",
                Name = "EventHandlersRegistration",
                Initializer = eventHandlerRegistrations
            };
            var fileContents = LiquidEngine.Render(ctx, "CyrusInitializer");
            spc.AddSource("EventHandlerRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));
        }
    }
}