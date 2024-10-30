namespace DemoApp.Contracts.Customers;

public partial record struct AddCustomerCommand(Name Name, Address Address);
