using System.Threading;

namespace NForza.Cqrs;

public class QueryProcessor(QueryHandlerDictionary queryHandlerDictionary) : IQueryProcessor
{
    public TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler<TQuery, TResult>();
        return handler.Invoke(query, cancellationToken);
    }
}
