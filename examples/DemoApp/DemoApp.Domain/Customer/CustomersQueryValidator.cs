using System.Collections.Generic;
using DemoApp.Contracts;
using DemoApp.Contracts.Customers;
using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer;

public class CustomerByIdQueryValidator
{
    [Validator]
    public static IEnumerable<string> Validate(CustomerByIdQuery query)
    {
        if (query.Id != CustomerId.Empty)
        {
            yield return "Id is not valid";
        }
    }
}
