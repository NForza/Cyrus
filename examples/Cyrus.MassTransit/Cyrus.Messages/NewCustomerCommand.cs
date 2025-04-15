using NForza.Cyrus.Abstractions;

namespace Cyrus.Server;

[Command]
public record NewCustomerCommand(CustomerId Id);
