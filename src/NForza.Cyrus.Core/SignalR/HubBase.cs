using Microsoft.AspNetCore.SignalR;

namespace NForza.Cyrus.SignalR;

public class HubBase: Hub
{
    protected async Task SendEvents(IEnumerable<object> eventsToSend)
    {
        var tasks = eventsToSend.Select(eventToSend => Clients.Caller.SendAsync(eventToSend.GetType().Name.ToCamelCase(), eventToSend));
        await Task.WhenAll(tasks);  
    }

    protected async Task SendQueryResultReply(string queryName, object queryResult)
    {
        await Clients.Caller.SendAsync(queryName.ToCamelCase() + "Result", queryResult);
    }
}
