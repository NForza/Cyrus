using DemoApp.Contracts;
using NForza.Cqrs;

namespace DemoApp.Domain;

public class AddCustomerCommandHandler
{
    public Task<CommandResult> Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return Task.FromResult(CommandResult.CompletedSuccessfully);
    }
}
