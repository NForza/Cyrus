using System.ComponentModel;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Event(Local = true)]
[Description("Customer added event")]
public record CustomerAddedEvent(CustomerId Id, Name Name, Address Address);

