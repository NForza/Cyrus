using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query]
public record struct CustomerByIdQuery(CustomerId Id);
