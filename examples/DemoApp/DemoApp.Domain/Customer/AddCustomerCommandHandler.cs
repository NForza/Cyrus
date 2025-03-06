using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandHandler
{
    [CommandHandler]
    public (IResult Result, IEnumerable<object> Events) Handle(AddCustomerCommand command)
    {
        CustomerId id = new CustomerId();
        Console.WriteLine($"Customer created: {id} {command.Name}, {command.Address}");
        return (Results.Accepted("/customers/" + id), [new CustomerAddedEvent(id, command.Name, command.Address)]);
    }
}
