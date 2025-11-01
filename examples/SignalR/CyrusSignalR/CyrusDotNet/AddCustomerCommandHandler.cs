using System.Collections.Generic;
using NForza.Cyrus.Abstractions;

namespace CyrusSignalR;

public class AddCustomerCommandHandler
{
    [CommandHandler(Route = "/")]
    public (Result Result, IEnumerable<object> Messages) AddCustomer(AddCustomerCommand command)
    {
        return (new AcceptedResult(), [new CustomerCreatedEvent(command.CustomerId, command.Name)]);
    }

}
