using NForza.Cyrus.Abstractions;

namespace Cyrus.Messages;

[Event]
public record CustomerCreatedEvent(CustomerId Id);
