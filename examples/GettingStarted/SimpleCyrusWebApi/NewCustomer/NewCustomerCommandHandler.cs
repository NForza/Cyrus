using NForza.Cyrus.Cqrs;
using SimpleCyrusWebApi.Storage;

namespace SimpleCyrusWebApi.NewCustomer;

public class NewCustomerCommandHandler(DemoContext context)
{
    public async Task<CommandResult> Execute(NewCustomerCommand command)
    {
        context.Customers.Add(new Customer(command.Id, command.Name, command.Address));
        await context.SaveChangesAsync();
        return new CommandResult(new CustomerCreatedEvent(command.Id));
    }
}
