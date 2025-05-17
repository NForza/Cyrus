using NForza.Cyrus.Abstractions;

namespace CyrusSignalR;

[Command]
public record struct AddCustomerCommand(CustomerId CustomerId, Name Name);
