using DemoApp.Contracts;

namespace DemoApp.Domain.Customer;

public record struct UpdateCustomerCommand(CustomerId CustomerId, Name Name, string Address);

