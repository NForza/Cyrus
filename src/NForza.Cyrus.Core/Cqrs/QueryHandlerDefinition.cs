namespace NForza.Cyrus.Cqrs;

public record QueryHandlerDefinition(string Route, Func<IServiceProvider, object, CancellationToken, object> Handler);