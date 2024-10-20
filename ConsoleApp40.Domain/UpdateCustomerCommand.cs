using DemoApp.Contracts;

namespace DemoApp.Domain;

public record UpdateCustomerCommand(CustomerId CustomerId, Name Name, string Address);
