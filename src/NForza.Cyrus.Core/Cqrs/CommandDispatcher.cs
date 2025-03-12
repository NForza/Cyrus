using System.Collections;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IEnumerable<IEventBus> eventBuses, ICommandBus commandBus) : ICommandDispatcher
{
    public Task DispatchEvents(IEnumerable events)
    {
        Parallel.ForEach(eventBuses, async eventBus =>
        {
            foreach (var e in events)
            {
                await eventBus.Publish(e);
            }
        });
        return Task.CompletedTask;
    }
}