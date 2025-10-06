using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace NForza.Cyrus.Cqrs;

public class MassTransitEventBus(IBus bus) : IMessageBus
{
    public Task Publish(params IEnumerable<object> events)
    {
        var tasks = events.Select(async m => await bus.Publish(m));
        return Task.WhenAll(tasks);
    }
}
