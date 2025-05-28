using DemoApp.Contracts;
using NForza.Cyrus.Aggregates;

namespace DemoApp.Domain.Customer;

[AggregateRoot]
public record Customer([property: AggregateRootId] CustomerId Id, Name Name);

