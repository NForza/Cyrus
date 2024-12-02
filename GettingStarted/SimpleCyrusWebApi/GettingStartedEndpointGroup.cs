using NForza.Cyrus.WebApi;

namespace SimpleCyrusWebApi;

public class GettingStartedEndpointGroup: EndpointGroup
{
    public GettingStartedEndpointGroup(): base("")
    {
        QueryEndpoint<HelloWorldQuery>()
            .Get("/hello-world");
    }
}
