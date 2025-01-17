namespace DemoApp.Contracts.Customers;

public record CustomerAddedEvent(CustomerId Id, Name Name, Address Address);