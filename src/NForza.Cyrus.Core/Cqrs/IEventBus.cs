namespace NForza.Cyrus.Cqrs;

public interface IEventBus
{
    Task Publish(IEnumerable<object> events);
}