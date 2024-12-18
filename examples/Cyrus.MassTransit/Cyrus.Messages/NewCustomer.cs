﻿using NForza.Cyrus.TypedIds;

namespace Cyrus.Server;

[GuidId]
public partial record struct CustomerId;

public record NewCustomerCommand(CustomerId Id);

public record CustomerCreatedEvent(CustomerId Id);