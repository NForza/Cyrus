using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;

namespace DemoApp.WebApi.Tests;

internal class RecordingLocalEventBus(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory, ILogger<LocalEventBus> logger) : LocalEventBus(eventHandlerDictionary, serviceScopeFactory, logger)
{
    public List<object> Messages { get; private set; } = [];
    public T? GetMessage<T>() => Messages.OfType<T>().FirstOrDefault();
    public IEnumerable<T> GetMessages<T>() => Messages.OfType<T>();

    override public Task Publish(IEnumerable<object> messages)
    {
        Messages.AddRange(messages);
        return base.Publish(messages);
    }
}
