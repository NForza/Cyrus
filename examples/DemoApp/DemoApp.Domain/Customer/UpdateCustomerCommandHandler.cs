using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandHandler
{
    [CommandHandler]
    [Authorize(Roles = "Admin")]
    public static async Task<(IResult result, object[] events)> Execute(UpdateCustomerCommand command)
    {
        await Task.Delay(1000);
        Console.WriteLine($"Customer updated: {command.Id}, {command.Name}, {command.Address}");
        return (Results.Accepted("/customers/" + command.Id), [new CustomerUpdatedEvent(command.Id)]);
    }
}
