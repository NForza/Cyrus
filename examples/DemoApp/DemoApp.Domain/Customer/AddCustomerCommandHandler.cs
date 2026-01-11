using System;
using System.Threading.Tasks;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using Microsoft.AspNetCore.Mvc;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public partial class AddCustomerCommandHandler
{
    [CommandHandler(Route = "customers", Verb = HttpVerb.Post)]
    [ProducesResponseType(202)]
    [HandlerStep(nameof(ValidateCommand))]
    [HandlerStep(nameof(CreateCustomerContext))]
    [HandlerStep(nameof(CreateCustomer))]
    [HandlerStep(nameof(ReturnStatus))]
    public partial Task<Result> Handle(AddCustomerCommand command);

    private Result? ValidateCommand(AddCustomerCommand command)
    {
        if (command.CustomerType == CustomerType.Private && command.Address.Street.ToString().Length == 0)
        {
           return Result.BadRequest("Private customers must have a valid street address.");
        }
        return null;
    }

    private async Task<AddCustomerCommandContext> CreateCustomerContext(AddCustomerCommand command)
    {
        await Task.CompletedTask;
        CustomerId id = new();
        return new AddCustomerCommandContext
        {
            Id = id,
            Name = command.Name,
            Address = command.Address
        };
    }

    private void CreateCustomer(AddCustomerCommandContext ctx)
    {
        Console.WriteLine($"Customer created: {ctx.Id} {ctx.Name}, {ctx.Address}");
        ctx.Messages.Add(new CustomerAddedEvent(ctx.Id, ctx.Name, ctx.Address));
    }

    private Result ReturnStatus(AddCustomerCommandContext ctx)
        => Result.Accepted("/customers/" + ctx.Id);
}