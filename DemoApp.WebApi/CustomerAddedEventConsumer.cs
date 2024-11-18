using DemoApp.Contracts.Customers;
using MassTransit;

namespace DemoApp.WebApi;

public class CustomerAddedEventConsumer : IConsumer<CustomerAddedEvent>
{
    public Task Consume(ConsumeContext<CustomerAddedEvent> context)
    {
        Console.WriteLine($"Customer Added: {context.Message.Id}");
        return Task.CompletedTask;
    }
}
