using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using NForza.Cyrus.WebApi;
using NForza.Cyrus.WebApi.Policies;

namespace DemoApp.WebApi;

public class CustomerEndpointGroup : EndpointGroup
{
    public CustomerEndpointGroup() : base("Customers")
    {
        CommandEndpoint<AddCustomerCommand>()
            .Post("")
            .AcceptedOnEvent<CustomerAddedEvent>("/customers/{Id}")
            .OtherwiseFail();

        CommandEndpoint<UpdateCustomerCommand>()
            .Put("")
            .AcceptedOnEvent<CustomerUpdatedEvent>("/customers/{Id}")
            .OtherwiseFail();       
        
        QueryEndpoint<AllCustomersQuery>()
            .Get("");

        QueryEndpoint<CustomerByIdQuery>()
            .Get("{Id}");
    }
}
