using Cyrus.Server;
using NForza.Cyrus.Cqrs;

namespace Cyrus.Producer;

public class NewCustomerCommandHandler
{
    public CommandResult Execute(NewCustomerCommand command)
    {
        return new CommandResult(new CustomerCreatedEvent(command.Id));
    }
}
