## Adding metadata

* Add *any* valid ASP.NET Core attribute for Minimal APIs to a query or command handler. This example uses the [OutputCache] and [Tags] attributes:

```csharp
    [QueryHandler(Route = "/customers/{page}/{pageSize}")]
    [OutputCache(Duration = 60)]
    [Tags("Customer")]
    public Task<Customer[]> QueryAllCustomers(AllCustomersQuery query)
    {
        var customers = Enumerable.Range(1, 10)
            .Select(i => new Customer(new(), new Name($"Customer-{i}")))
            .ToArray();
        return Task.FromResult(customers);
    }
```

* Or defining the possible status codes for a handler:

```csharp
    [CommandHandler(Route = "customers", Verb = HttpVerb.Post)]
    [ProducesResponseType(202)]
    public (IResult Result, IEnumerable<object> Events) Handle(AddCustomerCommand command)
    {
        CustomerId id = new CustomerId();
        Console.WriteLine($"Customer created: {id} {command.Name}, {command.Address}");
        return (Results.Accepted("/customers/" + id), [new CustomerAddedEvent(id, command.Name, command.Address)]);
    }
```

* As stated above, *any* valid ASP.NET Core attribute for Minimal APIs can be applied to a query or command handler. 