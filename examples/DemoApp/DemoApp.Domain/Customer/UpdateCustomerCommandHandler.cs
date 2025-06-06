﻿using System;
using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandHandler
{
    [CommandHandler(Route = "customers/{Id}", Verb = HttpVerb.Put)]
    public static (IResult Result, object Messages) Execute(UpdateCustomerCommand command, Customer customer)
    {
        Console.WriteLine($"Customer updated: {command.Id}, {command.Name}, {command.Address}");
        return (Results.Accepted("/customers/" + command.Id), new CustomerUpdatedEvent(command.Id));
    }
}
