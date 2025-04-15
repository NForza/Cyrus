using Cyrus.Server;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;

namespace Cyrus.Producer;

public class NewCustomerCommandHandler
{
    [CommandHandler(Route = "/", Verb = HttpVerb.Post)]
    [Tags("Customer")]
    public (IResult result, object @event) Execute(NewCustomerCommand command)
    {
        return (Results.Accepted(),  new CustomerCreatedEvent(command.Id));
    }
}
