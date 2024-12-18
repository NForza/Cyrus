using NForza.Cyrus.Cqrs;

namespace Cyrus.Server;

public class NewCustomerCommandHandler 
{
    public CommandResult Execute(NewCustomerCommand command)
    {
        return new CommandResult(new CustomerCreatedEvent(command.Id));
    }
}
