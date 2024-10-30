using System.Threading;

namespace NForza.Cqrs;

public interface IQueryProcessor
{
    TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken);
}
