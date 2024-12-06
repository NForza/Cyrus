namespace SimpleCyrusWebApi.NewCustomer;

public class CustomerCreatedEventHandler
{
    public void Handle(CustomerCreatedEvent @event)
    {
        Console.WriteLine($"New Customer Created: {@event.Id}");
    }
}
