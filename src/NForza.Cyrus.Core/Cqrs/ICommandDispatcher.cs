namespace NForza.Cyrus.Cqrs;

public interface ICommandDispatcher
{
    Task<CommandResult> ExecuteInternalAsync(object command, CancellationToken cancellationToken);
    CommandResult ExecuteInternalSync(object command);
}