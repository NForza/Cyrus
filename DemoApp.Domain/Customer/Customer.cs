using DemoApp.Contracts;

namespace DemoApp.Domain.Customer;

public class Customer(CustomerId id, Name name)
{
    public CustomerId Id { get; } = id;
    public Name Name { get; } = name;
}
