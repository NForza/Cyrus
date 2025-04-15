using NForza.Cyrus.Abstractions;

namespace Cyrus.Server;

[Event]
public record CustomerCreatedEvent(CustomerId Id);
