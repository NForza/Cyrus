using System;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class DeleteCustomerCommandHandler
{
    [CommandHandler(Verb = HttpVerb.Delete, Route = "customers/{Id}")]
    public (Result Result, object Messages) Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return (Result.Accepted(), new CustomerDeletedEvent(command.Id));
    }
}
