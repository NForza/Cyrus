using System.Threading.Tasks;

namespace NForza.Cqrs;

public interface IEventBus
{
    Task Publish(object @event);
}