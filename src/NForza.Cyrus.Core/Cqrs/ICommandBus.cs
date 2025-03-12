namespace NForza.Cyrus.Cqrs;

public interface ICommandBus
{
    Task Execute(object command, CancellationToken cancellationToken);
}