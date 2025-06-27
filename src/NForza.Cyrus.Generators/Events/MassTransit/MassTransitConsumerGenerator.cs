using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events.MassTransit;

public class MassTransitConsumerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        var eventHandlers = cyrusGenerationContext.EventHandlers.Where(eh => !IsEventHandlerForLocalEvent(eh));
        if (eventHandlers.Any())
        {
            if (config.EventBus == EventBusType.MassTransit)
            {
                var sourceText = GenerateEventConsumers(eventHandlers, cyrusGenerationContext.LiquidEngine);
                spc.AddSource($"EventConsumers.g.cs", sourceText);
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