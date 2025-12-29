using System.Collections.Generic;
using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class UpdateCustomerCommandValidator
{
    [Validator]
    public IEnumerable<string> Validate(UpdateCustomerCommand command)
    {
        if (command.Id == CustomerId.Empty) yield return "Id can't be empty.";
        if (command.Name.IsEmpty()) yield return "Name can't be empty.";
        if (command.Address.Street.IsEmpty()) yield return "Street can't be empty.";
        if (!command.Address.StreetNumber.IsValid()) yield return "Street number must be greater than 0.";
    }
}
