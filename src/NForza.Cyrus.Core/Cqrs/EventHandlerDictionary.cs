namespace NForza.Cyrus.Cqrs
{
    public class EventHandlerDictionary : MultiMap<Type, EventHandlerDefinition>
    {
        public void AddEventHandler<T>(string handlerName, Action<IServiceProvider, object> handler)
        {
            Add(typeof(T), new(handlerName, handler));
        }

        public IEnumerable<Action<IServiceProvider, object>> GetEventHandlers<T>()
            => GetValues(typeof(T)).Select(e => e.Handler);

        public IEnumerable<Action<IServiceProvider, object>> GetEventHandlers(Type eventHandlerType)
            => GetValues(eventHandlerType).Select(e => e.Handler);
    }
}
