using System.Text;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Events;

public class EventConsumerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var eventHandlers = cyrusGenerationContext.EventHandlers;
        foreach (var eventHandler in eventHandlers)
        {
            var sourceText = GenerateEventConsumer(eventHandler, cyrusGenerationContext.LiquidEngine);
            spc.AddSource($"{eventHandler.ContainingType.ToFullName().Replace("global::", "")}_EventConsumer.g.cs", sourceText);
        }
    }

    private string GenerateEventConsumer(IMethodSymbol eventHandler, LiquidEngine liquidEngine)
    {
        var model = new
        {
            Name = eventHandler.Parameters[0].Type.Name,
            FullName = eventHandler.Parameters[0].Type.ToFullName(),
            InvocationLambda = eventHandler.GetEventLambda("services") 
        };

        var resolvedSource = liquidEngine.Render(model, "EventConsumer");

        return resolvedSource;
    }
}