using NForza.Cyrus.Abstractions;

namespace Cyrus.Messages;

[Command]
public record NewCustomerCommand(CustomerId Id);
