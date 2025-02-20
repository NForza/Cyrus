using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.Cqrs;

public record CommandHandlerDefinition(string Route, HttpVerb Verb, Func<IServiceProvider, object, Task<CommandResult>> Handler);