using NForza.Cyrus.Abstractions;
using SimpleCyrusWebApi.Model;

namespace SimpleCyrusWebApi.NewCustomer;

[Command]
public class NewCustomerCommand
{
    public CustomerId Id { get; set; }
    public Name Name { get; set; }
    public Address Address { get; set; }
}
