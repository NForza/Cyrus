using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public record struct AugmentationResult(object? AugmentedObject, IResult? Result)
{
    public static AugmentationResult Success(object? augmentedObject)
    {
        return new AugmentationResult(augmentedObject, default);
    }

    public static AugmentationResult Failed(IResult result)
    {
        return new AugmentationResult(default, result);
    }
}