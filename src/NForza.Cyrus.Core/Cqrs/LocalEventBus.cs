using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class LocalEventBus(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory) : IEventBus
{
    public virtual Task Publish(IEnumerable<object> events)
    {
        var scope = serviceScopeFactory.CreateScope();
        foreach (var @event in events)
        {
            var handlers = eventHandlerDictionary.GetEventHandlers(@event.GetType());
            foreach (var handler in handlers)
            {
                handler.Invoke(scope.ServiceProvider, @event);
            }
        }
        return Task.CompletedTask;
    }
}