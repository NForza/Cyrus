using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class AugmentInputPolicyFunc(Func<object?, HttpContext, Task<AugmentationResult>> augmentFunction) : AugmentInputPolicy
{
    public override async Task<AugmentationResult> AugmentAsync(object? input, HttpContext httpContext)
    {
        var result = await augmentFunction(input, httpContext);
        return result;
    }
}