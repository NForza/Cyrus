using FluentValidation;

namespace DemoApp.Contracts.Customers;

public class AddCustomerCommandValidator : AbstractValidator<AddCustomerCommand>
{
    public AddCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Address).NotNull().NotEmpty();
    }
}
