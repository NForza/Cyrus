using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class LocalEventBus(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory) : IEventBus
{
    public Task Publish(object @event)
    {
        var scope = serviceScopeFactory.CreateScope();
        var handlers = eventHandlerDictionary.GetEventHandlers(@event.GetType());
        foreach (var handler in handlers)
        {
            handler.Invoke(scope.ServiceProvider, @event);
        }
        return Task.CompletedTask;
    }
}