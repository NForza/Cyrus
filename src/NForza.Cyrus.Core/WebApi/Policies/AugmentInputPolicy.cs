using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies
{
    public class AugmentPolicy
    {
        public virtual Task<object?> AugmentAsync(object? input, HttpContext httpContext) => Task.FromResult(input);
    }

    public class LambdaAugmentPolicy<T>(Func<T?, HttpContext, Task<T?>> augmentFunction) : AugmentPolicy
    {
        public override async Task<object?> AugmentAsync(object? input, HttpContext httpContext)
        {
            T? result = await augmentFunction((T?)input, httpContext);
            return result;
        }
    }
}