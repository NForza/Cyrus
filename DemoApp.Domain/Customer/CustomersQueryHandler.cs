﻿using DemoApp.Contracts;
using DemoApp.Contracts.Customers;

namespace DemoApp.Domain.Customer;

public class CustomersQueryHandler
{
    public List<Customer> Query(AllCustomersQuery query)
    {
        var customers = Enumerable.Range(1, 10)
            .Select(i => new Customer(new(), new($"Customer-{i}")))
            .ToList();  
        return customers;
    }

    public static Customer Query(CustomerByIdQuery query)
    {
        return new(query.Id, new($"Customer-{query.Id}"));
    }
}
