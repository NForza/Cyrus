using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : SignalRHub
{
    public CustomerHub()
    {
        QueryMethodFor<AllCustomersQuery>();
        CommandMethodFor<AddCustomerCommand>();
    }
}
