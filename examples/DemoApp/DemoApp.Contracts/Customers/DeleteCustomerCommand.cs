using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command(Verb = HttpVerb.Delete, Route = "customers/{Id}")]
public partial record DeleteCustomerCommand(CustomerId Id);