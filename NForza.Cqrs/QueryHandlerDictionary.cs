using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public class QueryHandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, object>>
{
    internal Func<TQuery, CancellationToken, TResult> GetHandler<TQuery, TResult>()
    {
        return (query, cancellationToken) => default;
    }

    public void AddHandler<T>(Func<IServiceProvider, object, object> handler)
    {
        Add(typeof(T), (services, c) => handler(services, c));
    }
}