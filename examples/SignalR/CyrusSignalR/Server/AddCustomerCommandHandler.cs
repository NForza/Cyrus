using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace CyrusSignalR;

public class AddCustomerCommandHandler
{
    [CommandHandler(Route = "/")]
    public (IResult Result, IEnumerable<object> Messages) AddCustomer(AddCustomerCommand command)
    {
        return (Results.Accepted(), [new CustomerCreatedEvent(command.CustomerId, command.Name)]);
    }

}
