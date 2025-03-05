using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;

namespace DemoApp.Domain.Customer;

public class DeleteCustomerCommandHandler
{
    [CommandHandler]
    public void Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
    }
}
