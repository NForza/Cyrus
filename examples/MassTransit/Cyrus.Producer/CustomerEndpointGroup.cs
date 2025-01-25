using NForza.Cyrus.WebApi;
using NForza.Cyrus.WebApi.Policies;

namespace Cyrus.Server;

public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup()  : base("Customers")
    {
        CommandEndpoint<NewCustomerCommand>()
             .Post("")
             .AcceptedOnEvent<CustomerCreatedEvent>("customers/{Id}")
             .OtherwiseFail();
    }
}
