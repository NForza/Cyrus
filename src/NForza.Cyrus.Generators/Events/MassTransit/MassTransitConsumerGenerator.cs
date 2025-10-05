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
        var eventHandlers = cyrusGenerationContext.EventHandlers;
        foreach (var eventHandler in eventHandlers)
        {
            var sourceText = GenerateEventConsumer(eventHandler, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{eventHandler.Parameters[0].Type.ToFullName().Replace("global::", "")}_EventConsumer.g.cs", sourceText);
        }
    }

    private string GenerateEventConsumer(IMethodSymbol eventHandler, LiquidEngine liquidEngine)
    {
        StringBuilder source = new();
        var model = new
        {
            Name = eventHandler.Parameters[0].Type.Name,
            FullName = eventHandler.Parameters[0].Type.ToFullName(),
            InvocationLambda = eventHandler.GetEventLambda("services") 
        };

        var resolvedSource = liquidEngine.Render(model, "EventConsumers");

        return resolvedSource;
    }
}