using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cqrs;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandHandler
{
    public Task<CommandResult> Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        CommandResult result = CommandResult.CompletedSuccessfully;
        result.AddEvent(new CustomerAddedEvent(new CustomerId(), command.Name, command.Address ));
        return Task.FromResult(result);
    }
}
