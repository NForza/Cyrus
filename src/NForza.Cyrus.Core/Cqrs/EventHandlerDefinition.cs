namespace NForza.Cyrus.Cqrs
{
    public record EventHandlerDefinition(string HandlerName, Action<IServiceProvider, object> Handler)
    {
    }
}