namespace NForza.Cyrus.Cqrs;

public class CommandHandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, Task<CommandResult>>>
{
    public void AddHandler<T>(Func<IServiceProvider, object, Task<CommandResult>> handler)
    {
        Add(typeof(T), (services, c) => handler(services, c));
    }
}