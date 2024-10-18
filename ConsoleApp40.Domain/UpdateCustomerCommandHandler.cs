using ConsoleApp40.Contracts;
using NForza.Cqrs;

namespace ConsoleApp40.Domain;

public class UpdateCustomerCommandHandler
{
    public Task<CommandResult> Execute(UpdateCustomerCommand command)
    {
        Console.WriteLine($"Customer updated: {command.CustomerId}, {command.Name}, {command.Address}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
}
