using Cyrus.Server;
using NForza.Cyrus.Abstractions;

namespace Cyrus.Consumer;

public class ConsumerEventHandler
{
    [EventHandler]
    public void Handle(CustomerCreatedEvent @event)
    {
        Console.WriteLine($"Customer created: {@event.Id}");
    }
}
