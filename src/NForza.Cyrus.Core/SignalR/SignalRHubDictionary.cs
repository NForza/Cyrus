using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace NForza.Cyrus.SignalR;

public class SignalRHubDictionary : Dictionary<string, Func<IEndpointRouteBuilder, HubEndpointConventionBuilder>>
{
    public void AddSignalRHub<T>(string path) where T : Hub
    {
        Add(path, endpoints => endpoints.MapHub<T>(path));
    }
}
