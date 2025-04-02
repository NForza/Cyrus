using MassTransit;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.MassTransit;

public class MassTransitEventBus(IBus bus, ILogger<MassTransitEventBus> logger) : IEventBus
{
    public Task Publish(IEnumerable<object> events)
    {
        return Task.WhenAll(events.Select(@event => 
            { 
                logger.LogDebug("Publishing event: {Event}", @event);
                return bus.Publish(@event); 
            }));
    }
}
