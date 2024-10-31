using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public class QueryHandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, object>>
{
    internal Func<TQuery, CancellationToken, TResult> GetHandler<TQuery, TResult>(IServiceProvider serviceProvider)
    {
        var func = this[typeof(TQuery)];
        func ??= (services, c) => throw new InvalidOperationException($"No handler found for query {typeof(TQuery).Name}");
        return (q, c) => (TResult)func(serviceProvider, q);
    }

    public void AddHandler<T>(Func<IServiceProvider, object, object> handler)
    {
        Add(typeof(T), (services, c) => handler(services, c));
    }
}