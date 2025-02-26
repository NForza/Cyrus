using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query(Route = "/customers/{page}/{pageSize}")]
public record struct AllCustomersQuery(int page = 1 , long pageSize = 10);
