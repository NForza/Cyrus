using MassTransit;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.MassTransit
{
    public class MassTransitEventBus(IBus bus) : IEventBus
    {
        public Task Publish(object @event)
        {
            return bus.Publish(@event);
        }
    }
}
