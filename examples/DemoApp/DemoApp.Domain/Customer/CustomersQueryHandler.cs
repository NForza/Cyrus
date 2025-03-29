using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.OutputCaching;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class CustomersQueryHandler
{
    [QueryHandler]
    [OutputCache(Duration = 60)]
    public Task<Customer[]> QueryAllCustomers(AllCustomersQuery query)
    {
        var customers = Enumerable.Range(1, 10)
            .Select(i => new Customer(new(), new Name($"Customer-{i}")))
            .ToArray();
        return Task.FromResult(customers);
    }

    [QueryHandler]
    public static Customer QueryCustomerById(CustomerByIdQuery query)
    {
        return new(query.Id, new($"Customer-{query.Id}"));
    }
}
