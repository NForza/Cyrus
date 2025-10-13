using NForza.Cyrus.Abstractions;

namespace CyrusSignalR;

[Command]
public record AddCustomerCommand(CustomerId CustomerId, Name Name);
