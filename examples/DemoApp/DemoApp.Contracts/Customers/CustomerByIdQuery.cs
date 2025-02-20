using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query(Route = "{Id:guid}")]
public record struct CustomerByIdQuery(CustomerId Id);
