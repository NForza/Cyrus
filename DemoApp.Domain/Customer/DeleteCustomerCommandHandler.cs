using DemoApp.Contracts.Customers;
using NForza.Cqrs;

namespace DemoApp.Domain.Customer;

public class DeleteCustomerCommandHandler
{
    public Task<CommandResult> Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
}
