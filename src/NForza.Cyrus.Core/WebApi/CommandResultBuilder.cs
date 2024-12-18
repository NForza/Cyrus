using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public class CommandResultBuilder(ICommandEndpointDefinition endpointDefinition)
{
    public CommandResultBuilder Tags(params string[] tags)
    {
        endpointDefinition.Tags = tags;
        return this;
    }

    internal CommandResultBuilder AddResultPolicy(CommandResultPolicy commandResultPolicy)
    {
        endpointDefinition.CommandResultPolicies.Add(commandResultPolicy);
        return this;
    }
}