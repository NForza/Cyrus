using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events.MassTransit;

public class MassTransitConsumerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        var config = cyrusProvider.GenerationConfig;
        var eventHandlers = cyrusProvider.EventHandlers.Where(eh => !IsEventHandlerForLocalEvent(eh));
        if (eventHandlers.Any())
        {
            if (config.EventBus == EventBusType.MassTransit)
            {
                var sourceText = GenerateEventConsumers(eventHandlers, cyrusProvider.LiquidEngine);
                spc.AddSource($"EventConsumers.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        }
    }

    private static bool IsEventHandlerForLocalEvent(IMethodSymbol eh)
    {
        return ((INamedTypeSymbol)eh.Parameters[0].Type).IsLocalEvent();
    }

    private string GenerateEventConsumers(IEnumerable<IMethodSymbol> eventHandlers, LiquidEngine liquidEngine)
    {
        StringBuilder source = new();
        var model = new
        {
            Consumers = eventHandlers.Select(h => new { h.Parameters[0].Type.Name, FullName = h.Parameters[0].Type.ToFullName() })
        };

        var resolvedSource = liquidEngine.Render(model, "EventConsumers");

        return resolvedSource;
    }
}