using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Query]
public record struct AllCustomersQuery(int page = 1 , long pageSize = 10);
