using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

[Command]
public record struct UpdateCustomerCommand(CustomerId CustomerId, Name Name, string Address);

