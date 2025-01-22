namespace NForza.Cyrus.Cqrs;

public record CommandHandlerDefinition(string HandlerName, Func<IServiceProvider, object, Task<CommandResult>> Handler)
{
}