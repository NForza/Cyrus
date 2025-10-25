using System;
using System.Threading.Tasks;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.WebApi;

public class CustomerEventHandler
{
    [EventHandler]
    public async Task Handle(CustomerAddedEvent @event)
    {
        await Task.Delay(10);
        Console.WriteLine($"Customer Added: {@event.Id}");
    }
}