using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[IntValue(minimum: 0, maximum: 100)]
public partial record struct Amount;