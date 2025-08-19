using System;
using System.Collections.Generic;

namespace NForza.Cyrus.Cqrs;

public class EventHandlerDictionary : MultiMap<Type, Action<IServiceProvider, object>>
{
    public void AddEventHandler<T>(Action<IServiceProvider, object> handler)
    {
        Add(typeof(T), handler);
    }

    public HashSet<Type> GetEventTypes()
    {
        var eventTypes = new HashSet<Type>();
        foreach (var key in Keys)
        {
            eventTypes.Add(key);
        }
        return eventTypes;
    }

    public IEnumerable<Action<IServiceProvider, object>> GetEventHandlers<T>()
        => GetValues(typeof(T));

    public IEnumerable<Action<IServiceProvider, object>> GetEventHandlers(Type eventHandlerType)
        => GetValues(eventHandlerType);
}
