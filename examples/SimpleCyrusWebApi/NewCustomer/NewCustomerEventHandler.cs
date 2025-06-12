using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi.NewCustomer;

public class CustomerCreatedEventHandler
{
    [EventHandler]
    public void Handle(CustomerCreatedEvent @event)
    {
        Console.WriteLine($"New Customer Created: {@event.Id}");
    }
}