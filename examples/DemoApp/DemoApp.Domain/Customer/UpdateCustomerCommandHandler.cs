using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandHandler
{
    [CommandHandler]
    public static (IResult result, object[] events) Execute(UpdateCustomerCommand command)
    {
        Console.WriteLine($"Customer updated: {command.Id}, {command.Name}, {command.Address}");
        return (Results.Accepted("/customers/" + command.Id), [new CustomerUpdatedEvent(command.Id)]);
    }
}
