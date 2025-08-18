using System.ComponentModel;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event]
[Description("Customer Deleted Event")]
public record CustomerDeletedEvent(CustomerId CustomerId);