using DemoApp.Contracts;

namespace DemoApp.Domain;

public record struct UpdateCustomerCommand(CustomerId CustomerId, Name Name, string Address);

