using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandValidator 
{
    [Validator]
    public IEnumerable<string> Validate(AddCustomerCommand command)
    {
        if (command.Id == CustomerId.Empty) yield return "Id can't be empty.";
        if (command.Name.IsEmpty()) yield return "Name can't be empty.";
        if (command.Address.Street.IsEmpty()) yield return "Street can't be empty.";
        if (!command.Address.StreetNumber.IsValid()) yield return "Street number must be greater than 0.";
    }
}
