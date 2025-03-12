using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

[Command(Route = "customers/{Id}", Verb = HttpVerb.Put)]
public record struct UpdateCustomerCommand(CustomerId CustomerId, Name Name, Address Address);

