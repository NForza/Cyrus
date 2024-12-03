using MassTransit;

namespace NForza.Cyrus.Cqrs.MassTransit
{
    public class MassTransitEventBus(IBus bus) : IEventBus
    {
        public Task Publish(object @event)
        {
            return bus.Publish(@event);
        }
    }
}
