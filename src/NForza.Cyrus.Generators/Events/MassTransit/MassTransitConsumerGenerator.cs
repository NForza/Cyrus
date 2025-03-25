using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events.MassTransit;

public class MassTransitConsumerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var config = cyrusProvider.GenerationConfig;
        var queryHandlers = cyrusProvider.QueryHandlers;
        if (queryHandlers.Any())
        {
            if (config.EventBus == "MassTransit")
            {
                var sourceText = GenerateEventConsumers(queryHandlers);
                spc.AddSource($"EventConsumers.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        }
    }

    private string GenerateEventConsumers(ImmutableArray<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        var model = new
        {
            Consumers = handlers.Select(h => new { h.Parameters[0].Type.Name, FullName = h.Parameters[0].Type.ToFullName() })
        };

        var resolvedSource = LiquidEngine.Render(model, "EventConsumers");

        return resolvedSource;
    }
}