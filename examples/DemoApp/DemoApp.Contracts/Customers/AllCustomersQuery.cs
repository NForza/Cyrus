using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query]
public record struct AllCustomersQuery(int Page = 1, int PageSize = 10);
