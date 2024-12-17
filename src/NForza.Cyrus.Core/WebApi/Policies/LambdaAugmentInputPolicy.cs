using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class InputPolicyFunc<T>(Func<T?, HttpContext, Task<T?>> augmentFunction) : InputPolicy
{
    public override async Task<object?> AugmentAsync(object? input, HttpContext httpContext)
    {
        T? result = await augmentFunction((T?)input, httpContext);
        return result;
    }
}