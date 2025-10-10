using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Aggregates;

namespace DemoApp.Domain.Customer;

[Command]
public record UpdateCustomerCommand([property: AggregateRootId] CustomerId Id, Name Name, Address Address);

