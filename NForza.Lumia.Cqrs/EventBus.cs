using System.Threading.Tasks;

namespace NForza.Lumia.Cqrs;

public class LocalEventBus : IEventBus
{
    public Task Publish(object @event)
    {
        return Task.CompletedTask;
    }
}