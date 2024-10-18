using ConsoleApp40.Contracts;
using NForza.Cqrs;

namespace ConsoleApp40.Domain;

public class AddCustomerCommandHandler
{
    public Task<CommandResult> Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
 }
