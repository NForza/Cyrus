
using System.ComponentModel;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event]
[Description("Customer Updated Event")]
public record CustomerUpdatedEvent(CustomerId Id);