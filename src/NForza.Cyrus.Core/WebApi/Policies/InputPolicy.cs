using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class InputPolicy
{
    public virtual Task<object?> AugmentAsync(object? input, HttpContext httpContext) => Task.FromResult(input);
}