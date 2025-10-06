using System;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandHandler
{
    [CommandHandler(Route = "customers/{Id}", Verb = HttpVerb.Put)]
    public static (Result Result, object Messages) Execute(UpdateCustomerCommand command, Customer customer)
    {
        Console.WriteLine($"Customer updated: {command.Id}, {command.Name}, {command.Address}");
        return (Result.Accepted("/customers/" + command.Id), new CustomerUpdatedEvent(command.Id));
    }
}
