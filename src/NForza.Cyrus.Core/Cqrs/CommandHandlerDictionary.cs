using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.Cqrs;

public class CommandHandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, Task<object>>>
{
    public void AddHandler<T>(string route, HttpVerb verb, Func<IServiceProvider, object, Task<object>> handler)
    {
        Add(typeof(T), handler);
    }
}