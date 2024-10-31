using System;
using System.Threading;

namespace NForza.Cqrs;

public class QueryProcessor(QueryHandlerDictionary queryHandlerDictionary, IServiceProvider serviceProvider) : IQueryProcessor
{
    public TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler<TQuery, TResult>(serviceProvider);
        return handler.Invoke(query, cancellationToken);
    }
}
