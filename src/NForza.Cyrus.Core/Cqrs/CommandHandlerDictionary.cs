using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.Cqrs;

public class CommandHandlerDictionary : Dictionary<Type, CommandHandlerDefinition>
{
    public void AddHandler<T>(string route, HttpVerb verb, Func<IServiceProvider, object, Task<CommandResult>> handler)
    {
        Add(typeof(T), new (route, verb, (services, c) => handler(services, c)));
    }
}