using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi.NewCustomer;

[Event]
public record CustomerCreatedEvent(CustomerId Id);