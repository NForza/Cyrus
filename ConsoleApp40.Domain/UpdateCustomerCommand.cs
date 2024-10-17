namespace ConsoleApp40.Contracts;

public record UpdateCustomerCommand(int CustomerId, string Name, string Address);
