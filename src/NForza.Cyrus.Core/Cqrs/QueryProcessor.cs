using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class QueryProcessor(QueryHandlerDictionary queryHandlerDictionary, IServiceScopeFactory serviceScopeFactory) : IQueryProcessor
{
    public Task<TResult> QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = queryHandlerDictionary.GetHandler<TQuery, TResult>(scope.ServiceProvider);
        return handler.Invoke(query, cancellationToken);
    }
    
    public Task<object> QueryInternal(object query, Type queryType, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = queryHandlerDictionary.GetHandler(scope.ServiceProvider, queryType);
        return (Task<object>) handler.Invoke(query, cancellationToken);
    }
}
