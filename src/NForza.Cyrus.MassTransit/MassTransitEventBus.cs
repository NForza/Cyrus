﻿using MassTransit;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.MassTransit;

public class MassTransitEventBus(IBus bus) : IEventBus
{
    public Task Publish(IEnumerable<object> events)
    {
        return Task.WhenAll(events.Select(@event => bus.Publish(@event)));
    }
}
