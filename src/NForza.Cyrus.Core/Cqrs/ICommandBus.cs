namespace NForza.Cyrus.Cqrs;

public interface ICommandBus
{
    Task<CommandResult> Execute(object command, CancellationToken cancellationToken);
}