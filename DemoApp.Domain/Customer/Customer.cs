using DemoApp.Contracts;

namespace DemoApp.Domain.Customer;

public class Customer(CustomerId id, string name)
{
    public CustomerId Id { get; } = id;
    public string Name { get; } = name;
}
