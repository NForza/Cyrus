using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event]
public record CustomerAddedEvent(CustomerId Id, Name Name, Address Address);

