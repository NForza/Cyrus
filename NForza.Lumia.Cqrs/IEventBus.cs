using System.Threading.Tasks;

namespace NForza.Lumia.Cqrs;

public interface IEventBus
{
    Task Publish(object @event);
}