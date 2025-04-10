﻿using DemoApp.Contracts;
using Microsoft.AspNetCore.Authorization;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

[Command]
public record struct UpdateCustomerCommand(CustomerId Id, Name Name, Address Address);

