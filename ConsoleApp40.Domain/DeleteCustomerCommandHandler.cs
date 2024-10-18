using ConsoleApp40.Contracts;
using NForza.Cqrs;

namespace ConsoleApp40.Domain;

public class DeleteCustomerCommandHandler
{
    public Task<CommandResult> Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
}
