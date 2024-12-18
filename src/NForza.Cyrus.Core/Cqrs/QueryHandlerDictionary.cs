﻿namespace NForza.Cyrus.Cqrs;

public class QueryHandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, CancellationToken, object>>
{
    private Dictionary<Type, Type> returnTypes = new();
    internal Func<TQuery, CancellationToken, Task<TResult>> GetHandler<TQuery, TResult>(IServiceProvider serviceProvider)
    {
        var func = this[typeof(TQuery)];
        func ??= (services, q, c) => throw new InvalidOperationException($"No handler found for query {typeof(TQuery).Name}");
        return async (query, cancellationToken) => (TResult)await (Task<object>)func(serviceProvider, query, cancellationToken);
    }

    internal Func<object, CancellationToken, object> GetHandler(IServiceProvider serviceProvider, Type queryType)
    {
        var func = this[queryType];
        return (q, c) => func(serviceProvider, q, c);
    }

    public Type GetQueryReturnType(Type queryType)
    {
        return returnTypes[queryType];
    }

    public void AddHandler<TQuery, TResult>(Func<IServiceProvider, object, CancellationToken, Task<object>> handler)
    {
        returnTypes.Add(typeof(TQuery), typeof(TResult));
        Add(typeof(TQuery), (services, query, token) => handler(services, query, token));
    }
}