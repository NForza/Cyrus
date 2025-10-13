using NForza.Cyrus.Abstractions;
using SimpleCyrusWebApi.Model;
using SimpleCyrusWebApi.Storage;

namespace SimpleCyrusWebApi.NewCustomer;

public class NewCustomerCommandHandler(DemoContext context)
{
    [CommandHandler(Route = "/")]
    public async Task<(Result Result, IEnumerable<object> events)> Execute(NewCustomerCommand command)
    {
        context.Customers.Add(new Customer(command.Id, command.Name, command.Address));
        await context.SaveChangesAsync();
        return (new OkResult(), [new CustomerCreatedEvent(command.Id)]);
    }
}
