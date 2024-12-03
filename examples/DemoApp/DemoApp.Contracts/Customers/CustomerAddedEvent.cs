namespace DemoApp.Contracts.Customers;

public record CustomerAddedEvent(CustomerId Id, Name name, Address address);