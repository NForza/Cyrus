using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event(Local = true)]
public record CustomerAddedEvent(CustomerId Id, Name Name, Address Address);

