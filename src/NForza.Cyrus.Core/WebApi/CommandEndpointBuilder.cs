using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public class CommandEndpointBuilder(ICommandEndpointDefinition endpointDefinition)
{
    public CommandEndpointBuilder Put(string path)
    {
        endpointDefinition.Method = "PUT";
        endpointDefinition.EndpointPath = path;
        return this;
    }

    public CommandEndpointBuilder Post(string path)
    {
        endpointDefinition.Method = "POST";
        endpointDefinition.EndpointPath = path;
        return this;
    }

    public CommandEndpointBuilder Tags(params string[] tags)
    {
        endpointDefinition.Tags = tags;
        return this;
    }

    public CommandEndpointBuilder MapInput<T>()
        where T : InputMappingPolicy
    {
        if (endpointDefinition.InputMappingPolicyType != null)
        {
            throw new InvalidOperationException($"Input mapping policy already set for {endpointDefinition.EndpointType.FullName}");
        }
        endpointDefinition.InputMappingPolicyType = typeof(T);
        return this;
    }

    internal CommandEndpointBuilder AddResultPolicy(CommandResultPolicy commandResultPolicy)
    {
        endpointDefinition.CommandResultPolicies.Add(commandResultPolicy);
        return this;
    }
}