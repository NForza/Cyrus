﻿using DemoApp.Contracts.Customers;
using NForza.Cyrus.SignalR;

namespace DemoApp.WebApi;

public class CustomerHub : SignalRHub
{
    public CustomerHub()
    {
        UsePath("/customerHub");
        QueryMethodFor<AllCustomersQuery>();
        CommandMethodFor<AddCustomerCommand>(replyToAllClients: true);
        CommandMethodFor<DeleteCustomerCommand>(replyToAllClients: true);

        PublishesEventToCaller<CustomerAddedEvent>();
        PublishesEventToAll<CustomerAddedEvent>();
    }
}
