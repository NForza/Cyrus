using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class CustomersQueryHandler
{
    [QueryHandler]
    public Task<Customer[]> Query(AllCustomersQuery query)
    {
        var customers = Enumerable.Range(1, 10)
            .Select(i => new Customer(new(), new Contracts.Name($"Customer-{i}")))
            .ToArray();
        return Task.FromResult(customers);
    }

    [QueryHandler]
    public static Customer Query(CustomerByIdQuery query)
    {
        return new(query.Id, new($"Customer-{query.Id}"));
    }
}
