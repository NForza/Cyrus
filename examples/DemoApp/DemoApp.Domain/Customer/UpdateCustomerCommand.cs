using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Aggregates;

namespace DemoApp.Domain.Customer;

[Command]
public record struct UpdateCustomerCommand([property: AggregateRootId] CustomerId Id, Name Name, Address Address);

