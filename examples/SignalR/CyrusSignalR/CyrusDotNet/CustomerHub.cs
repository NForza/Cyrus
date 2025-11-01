using NForza.Cyrus.SignalR;

namespace CyrusSignalR
{
    public class CustomerHub : CyrusSignalRHub
    {
        public CustomerHub()
        {
            UsePath("customer");
            Expose<AddCustomerCommand>();
            Emit<CustomerCreatedEvent>();
        }
    }
}
