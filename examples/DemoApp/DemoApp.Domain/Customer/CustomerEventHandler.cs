using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class CustomerEventHandler
{
    [EventHandler]
    public void Handle(CustomerAddedEvent @event)
    {
        Console.WriteLine($"Customer Added: {@event.Id}");
    }
}