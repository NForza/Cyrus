namespace NForza.Cyrus.Cqrs;

public class CommandHandlerDictionary : Dictionary<Type, CommandHandlerDefinition>
{
    public void AddHandler<T>(string handlerName, Func<IServiceProvider, object, Task<CommandResult>> handler)
    {
        Add(typeof(T), new (handlerName, (services, c) => handler(services, c)));
    }
}