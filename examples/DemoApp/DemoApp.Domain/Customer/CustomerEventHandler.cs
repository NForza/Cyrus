using DemoApp.Contracts.Customers;

namespace DemoApp.WebApi;

public class CustomerEventHandler
{
    public void Handle(CustomerAddedEvent @event)
    {
        Console.WriteLine($"Customer Added: {@event.Id}");
    }
}