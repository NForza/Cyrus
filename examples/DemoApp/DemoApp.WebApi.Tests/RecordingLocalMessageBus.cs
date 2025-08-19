using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NForza.Cyrus.Cqrs;

namespace DemoApp.WebApi.Tests;

internal class RecordingLocalMessageBus() : IMessageBus
{
    public List<object> Messages { get; private set; } = [];
    public T? GetMessage<T>() => Messages.OfType<T>().FirstOrDefault();
    public IEnumerable<T> GetMessages<T>() => Messages.OfType<T>();

    public Task Publish(params IEnumerable<object> messages)
    {
        Messages.AddRange(messages);
        return Task.CompletedTask;
    }
}
