using NForza.Cyrus.Cqrs;
using NForza.Cyrus.TypedIds;

namespace Cyrus.Server;

public class NewCustomerCommandHandler 
{
    public CommandResult Execute(NewCustomerCommand command)
    {
        return new CommandResult(new CustomerCreatedEvent(command.Id));
    }
}
