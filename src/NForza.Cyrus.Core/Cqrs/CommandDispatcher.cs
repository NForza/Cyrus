using System.Collections;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IEnumerable<IEventBus> eventBuses, IServiceProvider serviceProvider) : ICommandDispatcher
{
    public IServiceProvider ServiceProvider => serviceProvider;

    public Task DispatchEvents(IEnumerable<object> events)
    {
        Parallel.ForEach(eventBuses, async eventBus =>
        {
            await eventBus.Publish(events.OfType<object>());
        });
        return Task.CompletedTask;
    }
}