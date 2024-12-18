using Microsoft.AspNetCore.Http;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

#nullable enable

public class CommandEndpointBuilder<T>(ICommandEndpointDefinition endpointDefinition)
{
    public CommandResultBuilder Put(string path)
    {
        endpointDefinition.Method = "PUT";
        endpointDefinition.EndpointPath = path;
        return new(endpointDefinition);
    }

    public CommandResultBuilder Post(string path)
    {
        endpointDefinition.Method = "POST";
        endpointDefinition.EndpointPath = path;
        return new(endpointDefinition);
    }

    public CommandEndpointBuilder<T> AugmentInput(Func<object?, HttpContext, AugmentationResult> augmentFunc)
    {
        endpointDefinition.AugmentInputPolicies.Add(new AugmentInputPolicyFunc((c, ctx) => Task.FromResult(augmentFunc(c, ctx))));
        return this;
    }

    public CommandEndpointBuilder<T> MapInput<TInput>()
        where TInput : InputMappingPolicy
    {
        if (endpointDefinition.InputMappingPolicyType != null)
        {
            throw new InvalidOperationException($"Input mapping policy already set for {endpointDefinition.EndpointType.FullName}");
        }
        endpointDefinition.InputMappingPolicyType = typeof(TInput);
        return this;
    }

    public CommandResultBuilder Delete(string path)
    {
        endpointDefinition.Method = "DELETE";
        endpointDefinition.EndpointPath = path;
        return new(endpointDefinition);
    }
}

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