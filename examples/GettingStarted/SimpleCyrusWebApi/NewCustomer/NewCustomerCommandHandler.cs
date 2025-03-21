using System.Collections;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;
using SimpleCyrusWebApi.Model;
using SimpleCyrusWebApi.Storage;

namespace SimpleCyrusWebApi.NewCustomer;

public class NewCustomerCommandHandler(DemoContext context)
{
    [CommandHandler]
    public async Task<IEnumerable<object>> Execute(NewCustomerCommand command)
    {
        context.Customers.Add(new Customer(command.Id, command.Name, command.Address));
        await context.SaveChangesAsync();
        return [new CustomerCreatedEvent(command.Id)];
    }
}
