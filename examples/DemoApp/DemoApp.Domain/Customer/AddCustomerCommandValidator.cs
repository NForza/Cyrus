using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandValidator 
{
    [Validator]
    public IEnumerable<string> Validate(AddCustomerCommand command)
    {
        if (command.Id == CustomerId.Empty)
        {
            yield return "Id cannot be empty.";
        }
        if (command.Name.IsEmpty())
        {
            yield return "Name cannot be empty.";
        }
        if (command.Address.Street.IsEmpty())
        {
            yield return "Street cannot be empty.";
        }
        if (!command.Address.StreetNumber.IsValid())
        {
            yield return "StreetNumber is invalid.";
        }
    }
}
