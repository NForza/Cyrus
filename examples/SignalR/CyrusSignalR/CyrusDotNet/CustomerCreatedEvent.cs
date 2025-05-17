using NForza.Cyrus.Abstractions;

namespace CyrusSignalR;

[Event]
public record CustomerCreatedEvent(CustomerId customerId, Name name);