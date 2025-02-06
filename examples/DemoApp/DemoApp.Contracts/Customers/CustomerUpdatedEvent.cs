
using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event]
public record CustomerUpdatedEvent(CustomerId Id);