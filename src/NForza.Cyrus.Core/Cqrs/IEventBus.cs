namespace NForza.Cyrus.Cqrs;

public interface IEventBus
{
    Task Publish(params object[] events);
}