using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class DeleteCustomerCommandHandler
{
    [CommandHandler]
    public IResult Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return Results.Accepted();
    }
}
