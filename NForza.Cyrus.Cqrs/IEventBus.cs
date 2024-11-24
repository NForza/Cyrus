using System.Threading.Tasks;

namespace NForza.Cyrus.Cqrs;

public interface IEventBus
{
    Task Publish(object @event);
}