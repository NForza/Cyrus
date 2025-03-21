namespace NForza.Cyrus.Cqrs;

public class QueryHandlerDictionary : Dictionary<Type, QueryHandlerDefinition>
{
    private Dictionary<Type, Type> returnTypes = new();
    internal Func<TQuery, CancellationToken, Task<TResult>> GetHandler<TQuery, TResult>(IServiceProvider serviceProvider)
    {
        var func = this[typeof(TQuery)].Handler;
        func ??= (services, q, c) => throw new InvalidOperationException($"No handler found for query {typeof(TQuery).Name}");
        return async (query, cancellationToken) => (TResult)await (Task<object>)func(serviceProvider, query, cancellationToken);
    }

    internal Func<object, CancellationToken, object> GetHandler(IServiceProvider serviceProvider, Type queryType)
    {
        var func = this[queryType].Handler;
        return (q, c) => func(serviceProvider, q, c);
    }

    public Type GetQueryReturnType(Type queryType)
    {
        return returnTypes[queryType];
    }

    public void AddHandler<TQuery, TResult>(string route, Func<IServiceProvider, object, CancellationToken, Task<object>> handler)
    {
        returnTypes.Add(typeof(TQuery), typeof(TResult));
        Add(typeof(TQuery), new(route, (services, query, token) => handler(services, query, token)));
    }
}