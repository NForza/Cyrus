using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandHandler
{
    [CommandHandler]
    public static Task<CommandResult> Execute(UpdateCustomerCommand command)
    {
        Console.WriteLine($"Customer updated: {command.CustomerId}, {command.Name}, {command.Address}");
        return Task.FromResult(new CommandResult(new CustomerUpdatedEvent(command.CustomerId)));
    }
}
