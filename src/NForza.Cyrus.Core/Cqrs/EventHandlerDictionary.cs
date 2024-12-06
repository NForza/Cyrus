using NForza.Cyrus.Core;

namespace NForza.Cyrus.Cqrs
{
    public class EventHandlerDictionary: MultiMap<Type, Action<IServiceProvider, object>>
    {
        public void AddEventHandler<T>(Action<IServiceProvider, object> handler)
        {
            Add(typeof(T), handler);
        }

        public IEnumerable<Action<IServiceProvider, object>> GetEventHandlers<T>() 
            => GetValues(typeof(T));
    }
}
