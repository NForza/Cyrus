using Microsoft.AspNetCore.SignalR;

namespace NForza.Cyrus.SignalR;

public class HubBase: Hub
{
    protected async void SendEvents(IEnumerable<object> eventsToSend)
    {
        var tasks = eventsToSend.Select(eventToSend => Clients.Caller.SendAsync(eventToSend.GetType().Name.ToCamelCase(), eventToSend));
        await Task.WhenAll(tasks);  
    }
}
