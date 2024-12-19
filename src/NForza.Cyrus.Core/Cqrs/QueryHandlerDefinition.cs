namespace NForza.Cyrus.Cqrs;

public record QueryHandlerDefinition(string HandlerName, Func<IServiceProvider, object, CancellationToken, object> Handler);