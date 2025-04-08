using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command]
public partial record DeleteCustomerCommand(CustomerId? Id);