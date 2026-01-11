using DemoApp.Contracts;
using NForza.Cyrus.Cqrs;

namespace DemoApp.Domain.Customer;

internal class AddCustomerCommandContext : CqrsCommandContext
{
    public CustomerId Id { get; set; }
    public Name Name { get; set; }
    public Address Address { get; set; }
}
