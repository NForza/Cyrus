using DemoApp.Contracts.Customers;

namespace DemoApp.Domain.Customer;

public class CustomerEventHandler
{
    public void Handle(CustomerAddedEvent @event)
    {
        Console.WriteLine($"Customer Added: {@event.Id}");
    }
}