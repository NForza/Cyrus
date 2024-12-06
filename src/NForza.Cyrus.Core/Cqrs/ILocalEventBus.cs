
namespace NForza.Cyrus.Cqrs
{
    public interface ILocalEventBus
    {
        Task Publish(object @event);
    }
}