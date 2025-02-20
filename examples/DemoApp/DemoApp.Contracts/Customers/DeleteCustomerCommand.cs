using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command(Verb = HttpVerb.Delete, Route = "{Id}")]
public partial record DeleteCustomerCommand(CustomerId Id);