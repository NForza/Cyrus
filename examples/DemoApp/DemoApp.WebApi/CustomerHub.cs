using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : CyrusSignalRHub
{
    public CustomerHub()
    {
        UsePath("/customer-hub");
        Expose<AllCustomersQuery>();
        Expose<AddCustomerCommand>();
        Expose<DeleteCustomerCommand>();

        Send<CustomerUpdatedEvent>();
        Broadcast<CustomerAddedEvent>();
    }
}
