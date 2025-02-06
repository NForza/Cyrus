using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandHandler
{
    [CommandHandler]
    public CommandResult Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return new CommandResult(new CustomerAddedEvent(new CustomerId(), command.Name, command.Address));
    }
}
