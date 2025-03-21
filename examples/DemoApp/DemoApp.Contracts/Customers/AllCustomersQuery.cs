using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query(Route = "/customers/{page}/{pageSize}")]
public record struct AllCustomersQuery(int Page = 1 , long PageSize = 10);
