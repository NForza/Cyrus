using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query(Route = "customers/{Id:guid}")]
public record struct CustomerByIdQuery(CustomerId Id);
