using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NForza.Cyrus.SignalR;

[EditorBrowsable(EditorBrowsableState.Never)]
public class CyrusHub : Hub
{
    private readonly IReadOnlyDictionary<Type, bool> eventConfig;

    public CyrusHub(IEnumerable<(Type Type, bool Broadcast)> events)
    {
        eventConfig = events.ToDictionary(e => e.Type, e => e.Broadcast);
    }

    protected Task SendEvents(object eventToSend)
        => SendEvents([eventToSend]);

    protected async Task SendEvents(IEnumerable<object> eventsToSend)
    {
        var tasks = new List<Task>();

        foreach (var evt in eventsToSend)
        {
            if (!eventConfig.TryGetValue(evt.GetType(), out var broadcast))
                continue;

            var methodName = evt.GetType().Name.ToCamelCase();

            var sendTask = (broadcast ? Clients.All : Clients.Caller).SendAsync(methodName, evt);

            tasks.Add(sendTask);
        }
        await Task.WhenAll(tasks);
    }

    protected async Task SendQueryResultReply(string queryName, object queryResult)
    {
        await Clients.Caller.SendAsync(queryName.ToCamelCase() + "Result", queryResult);
    }
}
