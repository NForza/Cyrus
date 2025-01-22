using Cyrus.Server;
using NForza.Cyrus.WebApi;
using NForza.Cyrus.WebApi.Policies;

namespace Cyrus.Producer;

public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup() : base("Customers")
    {
        CommandEndpoint<NewCustomerCommand>()
             .Post("")
             .AcceptedOnEvent<CustomerCreatedEvent>("customers/{Id}")
             .OtherwiseFail();
    }
}
