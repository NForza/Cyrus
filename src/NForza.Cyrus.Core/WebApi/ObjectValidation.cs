using FluentValidation;
using FluentValidation.Results;

namespace NForza.Cyrus.WebApi
{
    public static class ObjectValidation
    {
        public static bool Validate<T>(object? queryObject, IServiceProvider serviceProvider, out object? problem)
        {
            Type objectType = typeof(T);
            if (objectType == null)
            {
                problem = "Can't create Query object of Type " + objectType?.Name;
                return false;
            }
            var validatorType = typeof(IValidator<>).MakeGenericType(objectType);
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                problem = null;
                return true;
            }

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(objectType);
            var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, queryObject)!;

            ValidationResult validationResult = validator?.Validate(validationContext) ?? throw new InvalidOperationException($"Can't validate {objectType.FullName}");
            if (validationResult.IsValid)
            {
                problem = null;
                return true;
            }
            problem = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return false;
        }
    }
}