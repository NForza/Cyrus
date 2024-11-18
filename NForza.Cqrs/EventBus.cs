using System.Threading.Tasks;

namespace NForza.Cqrs;

public class LocalEventBus : IEventBus
{
    public Task Publish(object @event)
    {
        return Task.CompletedTask;
    }
}