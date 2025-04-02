using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;

namespace DemoApp.WebApi.Tests;

internal class RecordingLocalEventBus(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory, ILogger<LocalEventBus> logger) : LocalEventBus(eventHandlerDictionary, serviceScopeFactory, logger)
{
    public List<object> Events { get; private set; } = [];
    public T? GetEvent<T>() => Events.OfType<T>().FirstOrDefault();
    public IEnumerable<T> GetEvents<T>() => Events.OfType<T>();

    override public Task Publish(IEnumerable<object> events)
    {
        Events.AddRange(events);
        return base.Publish(events);
    }
}
