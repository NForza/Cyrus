using System;
using System.Threading;

namespace NForza.Cyrus.Cqrs;

public class QueryProcessor(QueryHandlerDictionary queryHandlerDictionary, IServiceProvider serviceProvider) : IQueryProcessor
{
    public Task<TResult> QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler<TQuery, TResult>(serviceProvider);
        return handler.Invoke(query, cancellationToken);
    }
    
    public Task<object> QueryInternal(object query, Type queryType, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler(serviceProvider, queryType);
        return (Task<object>) handler.Invoke(query, cancellationToken);
    }
}
