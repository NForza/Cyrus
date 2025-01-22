using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : SignalRHub
{
    public CustomerHub()
    {
        UsePath("/customer-hub");
        QueryMethodFor<AllCustomersQuery>();
        CommandMethodFor<AddCustomerCommand>(replyToAllClients: true);
        CommandMethodFor<DeleteCustomerCommand>(replyToAllClients: true);

        PublishesEventToCaller<CustomerUpdatedEvent>();
        PublishesEventToAll<CustomerAddedEvent>();
    }
}
