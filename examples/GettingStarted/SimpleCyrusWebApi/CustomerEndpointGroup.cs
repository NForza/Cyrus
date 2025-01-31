using NForza.Cyrus.WebApi;
using NForza.Cyrus.WebApi.Policies;
using SimpleCyrusWebApi.CustomerById;
using SimpleCyrusWebApi.NewCustomer;

namespace SimpleCyrusWebApi;

public class CustomerEndpointGroup : EndpointGroup
{
    //exposes the CustomerByIdQuery on the endpoint /customer/{id} using a GET
    //The Id parameter is automatically deserialized from the route parameters
    //Customers is the tag for the endpoint group in Swagger
    // "/customers" is the base URL for the endpoint group
    public CustomerEndpointGroup() : base("Customers", "/customers")
    {
        //endpoint URL will be GET /customers/{Id}
        QueryEndpoint<CustomerByIdQuery>()
            .Get("{Id:guid}");


        //endpoint URL will be POST /customers
        CommandEndpoint<NewCustomerCommand>()
            .Post("")
            .AcceptedOnEvent<CustomerCreatedEvent>("/customers/{Id}")
            .OtherwiseFail();
    }
}
