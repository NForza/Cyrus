using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command]
public record struct AddCustomerCommand(Name Name, Address Address, CustomerType CustomerType);
