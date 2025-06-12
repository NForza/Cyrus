using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi.NewCustomer;

[Command]
public record struct NewCustomerCommand
{
    public CustomerId Id { get; set; }
    public Name Name { get; set; }
    public Address Address { get; set; }
}
