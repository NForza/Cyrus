using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : SignalRHub
{
    public CustomerHub()
    {
        UsePath("/customer-hub");
        QueryMethodFor<AllCustomersQuery>();
        CommandMethodFor<AddCustomerCommand>();
        CommandMethodFor<DeleteCustomerCommand>();

        PublishesEventToCaller<CustomerUpdatedEvent>();
        PublishesEventToAll<CustomerAddedEvent>();
    }
}
