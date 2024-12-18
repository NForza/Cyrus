using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class AugmentInputPolicy
{
    public virtual Task<AugmentationResult> AugmentAsync(object? input, HttpContext httpContext) => Task.FromResult(AugmentationResult.Success(input));
}