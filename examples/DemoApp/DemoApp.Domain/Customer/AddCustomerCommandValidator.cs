using DemoApp.Contracts.Customers;
using FluentValidation;

namespace DemoApp.Domain.Customer;

public class AddCustomerCommandValidator : AbstractValidator<AddCustomerCommand>
{
    public AddCustomerCommandValidator()
    {
        RuleFor(x => x.Name.Value).NotNull().NotEmpty().WithMessage("Name can't be empty");
        RuleFor(x => x.Address.Street.Value).NotNull().NotEmpty().WithMessage("Street can't be empty");
        RuleFor(x => x.CustomerType).NotNull().WithMessage("CustomerType can't be empty");
        RuleFor(x => x.Address.StreetNumber.Value).GreaterThan(0).WithMessage("Street number must be greater than 0");
    }
}
