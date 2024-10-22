using DemoApp.Contracts;
using NForza.Cqrs.WebApi;
using NForza.Cqrs.WebApi.Policies;

namespace DemoApp.WebApi
{
    public class CustomerEndpointGroup : EndpointGroup
    {
        public CustomerEndpointGroup() : base("Customers")
        {
            CommandEndpoint<AddCustomerCommand>()
                .Post("")
                .AcceptedOnEvent<CustomerAddedEvent>("/customers/{Id}")
                .OtherwiseFail();
        }
    }
}
