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
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        CustomerId id = new CustomerId();
        return (Results.AcceptedAtRoute("/customers/"+id), [new CustomerAddedEvent(id, command.Name, command.Address)]);
    }
}
