using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using NForza.Lumia.Cqrs.WebApi;
using NForza.Lumia.Cqrs.WebApi.Policies;

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
