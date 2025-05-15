using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.MassTransit;

public class MassTransitEventBus(IBus bus, IServiceProvider services, LocalEventList localEvents, ILogger<MassTransitEventBus> logger) : IEventBus
{
    private LocalEventBus localEventBus = services.GetService<LocalEventBus>() ?? throw new InvalidOperationException("LocalEventBus not registered in DI container.");
    public Task Publish(IEnumerable<object> events)
    {
        var localEventsToPublish = events.Where(e => localEvents.Contains(e.GetType())).ToList();
        var eventsToPublish = events.Except(localEventsToPublish).ToList();

        localEventBus.Publish(localEventsToPublish);
        return Task.WhenAll(eventsToPublish.Select(@event => 
            { 
                logger.LogDebug("Publishing event: {Event}", @event);
                return bus.Publish(@event); 
            }));
    }
}
