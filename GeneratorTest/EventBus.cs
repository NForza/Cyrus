using System.Threading.Tasks;

namespace NForza.Cqrs;

public class EventBus : IEventBus
{
    public Task Publish(object @event)
    {
        return Task.CompletedTask;
    }
}