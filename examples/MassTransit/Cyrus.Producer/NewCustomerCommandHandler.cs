using Cyrus.Messages;
using NForza.Cyrus.Abstractions;

namespace Cyrus.Producer;

public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/", Verb = HttpVerb.Post)]
    [Tags("Customer")]
    public (Result Result, object Messages) Execute(NewCustomerCommand command)
    {
        Console.WriteLine("Executing handler for NewCustomerCommand");
        return (Result.Accepted(), new CustomerCreatedEvent(command.Id));
    }
}
