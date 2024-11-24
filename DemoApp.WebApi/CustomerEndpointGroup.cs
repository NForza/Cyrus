using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using NForza.Cyrus.Cqrs.WebApi;
using NForza.Cyrus.Cqrs.WebApi.Policies;

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
            .Get("/customers");

        QueryEndpoint<CustomerByIdQuery>()
            .Get("/customers/{Id}");
    }
}
