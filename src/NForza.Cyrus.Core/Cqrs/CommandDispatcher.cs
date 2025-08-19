using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IEnumerable<IMessageBus> eventBuses, IServiceProvider serviceProvider) : ICommandDispatcher
{
    public IServiceProvider Services => serviceProvider;

    public Task DispatchEvents(IEnumerable<object> events)
    {
        Parallel.ForEach(eventBuses, async eventBus =>
        {
            await eventBus.Publish(events.OfType<object>());
        });
        return Task.CompletedTask;
    }
}