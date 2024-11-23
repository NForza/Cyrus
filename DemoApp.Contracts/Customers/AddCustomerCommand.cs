namespace DemoApp.Contracts.Customers;

public record struct AddCustomerCommand(Name Name, Address Address);
