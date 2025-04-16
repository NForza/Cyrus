using Cyrus.Messages;
using NForza.Cyrus.Abstractions;

namespace Cyrus.Producer;

public class NewCustomerCommandValidator
{
    [Validator]
    public IEnumerable<string> Validate(NewCustomerCommand command)
    {
        if (command.Id == CustomerId.Empty)
        {
            yield return "Id is required.";
        }
    }
}
