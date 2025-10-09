using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command]
public record AddCustomerCommand(CustomerId Id, Name Name, Address Address, CustomerType CustomerType);