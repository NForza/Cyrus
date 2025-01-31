using DemoApp.Contracts.Customers;
using FluentValidation;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandValidator : AbstractValidator<AddCustomerCommand>
{
    public AddCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Address).NotNull().NotEmpty();
    }
}
