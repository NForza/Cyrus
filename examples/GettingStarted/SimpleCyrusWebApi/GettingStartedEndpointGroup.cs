using NForza.Cyrus.WebApi;

namespace SimpleCyrusWebApi;

public class GettingStartedEndpointGroup: EndpointGroup
{
    //exposes the CustomerByIdQuery on the endpoint /customer/{id} using a GET
    //The Id parameter is automatically deserialized from the route parameters
    public GettingStartedEndpointGroup(): base("")
    {
        QueryEndpoint<CustomerByIdQuery>()
            .Get("/customer/{Id}");
    }
}
