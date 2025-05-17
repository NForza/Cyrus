using NForza.Cyrus.SignalR;

namespace CyrusSignalR
{
    public class CustomerHub: CyrusSignalRHub
    {
        public CustomerHub()
        {
            Expose<AddCustomerCommand>();
            Emit<CustomerCreatedEvent>();
        }
    }
}
