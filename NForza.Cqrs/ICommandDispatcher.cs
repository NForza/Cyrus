using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public interface ICommandDispatcher
{
    Task<CommandResult> ExecuteInternal(object command, CancellationToken cancellationToken);
}