using DemoApp.Contracts;
using NForza.Cqrs;

namespace DemoApp.Domain;

public class DeleteCustomerCommandHandler
{
    public Task<CommandResult> Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
}
