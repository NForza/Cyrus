using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

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
}