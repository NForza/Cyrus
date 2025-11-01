using Cyrus.Messages;
using NForza.Cyrus.Abstractions;

namespace Cyrus.Producer;

[Command]
public record NewCustomerCommand(CustomerId Id);
