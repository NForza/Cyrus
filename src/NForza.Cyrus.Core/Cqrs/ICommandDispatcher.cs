namespace NForza.Cyrus.Cqrs;

public interface ICommandDispatcher
{
    IServiceProvider ServiceProvider { get; }
}