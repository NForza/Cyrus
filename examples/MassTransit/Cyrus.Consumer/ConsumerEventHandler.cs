using Cyrus.Server;

namespace Cyrus.Consumer;

public class ConsumerEventHandler
{
    public void Handle(CustomerCreatedEvent @event)
    {
        Console.WriteLine($"Customer created: {@event.Id}");
    }
}
