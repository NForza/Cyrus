using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : SignalRHub
{
    public CustomerHub()
    {
        UsePath("/customer-hub");
        Query<AllCustomersQuery>();
        Command<AddCustomerCommand>();
        Command<DeleteCustomerCommand>();

        PublishesEventToCaller<CustomerUpdatedEvent>();
        PublishesEventToAll<CustomerAddedEvent>();
    }
}
