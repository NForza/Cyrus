using System;
using System.Threading;

namespace NForza.Cyrus.Cqrs;

public class QueryProcessor(QueryHandlerDictionary queryHandlerDictionary, IServiceProvider serviceProvider) : IQueryProcessor
{
    public TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler<TQuery, TResult>(serviceProvider);
        return handler.Invoke(query, cancellationToken);
    }
    
    public object QueryInternal(object query, Type queryType, CancellationToken cancellationToken)
    {
        var handler = queryHandlerDictionary.GetHandler(serviceProvider, queryType);
        return handler.Invoke(query, cancellationToken);
    }
}
