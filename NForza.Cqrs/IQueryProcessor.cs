using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public interface IQueryProcessor
{
    Task<CommandResult> QueryInternal(object query, CancellationToken cancellationToken);
}