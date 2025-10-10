using System;
using System.Collections.Generic;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandHandler
{
    [CommandHandler(Route = "customers", Verb = HttpVerb.Post)]
    [Microsoft.AspNetCore.Mvc.ProducesResponseType(202)]
    public (Result Result, IEnumerable<object> Messages) Handle(AddCustomerCommand command)
    {
        CustomerId id = new CustomerId();
        Console.WriteLine($"Customer created: {id} {command.Name}, {command.Address}");

        return (new AcceptedResult(new() { Location = "/customers/" + id }), [new CustomerAddedEvent(id, command.Name, command.Address)]);
    }
}
