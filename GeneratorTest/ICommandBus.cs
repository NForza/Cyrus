using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public interface ICommandBus
{
    Task<CommandResult> Execute(object command, CancellationToken cancellationToken);
}