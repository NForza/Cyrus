using Cyrus.Messages;
using NForza.Cyrus.Abstractions;

namespace Cyrus.Producer;

public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/", Verb = HttpVerb.Post)]
    [Tags("Customer")]
    public (IResult result, object @event) Execute(NewCustomerCommand command)
    {
        Console.WriteLine("Executing handler for NewCustomerCommand");
        return (Results.Accepted(),  new CustomerCreatedEvent(command.Id));
    }
}
