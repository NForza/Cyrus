using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NForza.Cyrus.Cqrs;

public class LocalEventBus(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory, ILogger<LocalEventBus> logger) : IEventBus
{
    public virtual Task Publish(IEnumerable<object> events)
    {
        var scope = serviceScopeFactory.CreateScope();
        foreach (var @event in events)
        {
            var handlers = eventHandlerDictionary.GetEventHandlers(@event.GetType());
            logger.LogDebug("Found {Count} event handlers for event {Event}", handlers.Count(), @event.GetType().Name);
            foreach (var handler in handlers)
            {
                logger.LogDebug("Invoking event handler {Handler} for event {Event}", handler.Method.Name, @event.GetType().Name);
                handler.Invoke(scope.ServiceProvider, @event);
            }
        }
        return Task.CompletedTask;
    }
}