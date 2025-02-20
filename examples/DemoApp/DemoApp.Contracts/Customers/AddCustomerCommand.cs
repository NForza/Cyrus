using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts.Customers;

[Command(Route = "", Verb = HttpVerb.Post)]
public record struct AddCustomerCommand(Name Name, Address Address, CustomerType CustomerType);