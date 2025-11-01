using NForza.Cyrus.Abstractions;
using SimpleCyrusWebApi.Model;

namespace SimpleCyrusWebApi.NewCustomer;

[Event]
public record CustomerCreatedEvent(CustomerId Id);