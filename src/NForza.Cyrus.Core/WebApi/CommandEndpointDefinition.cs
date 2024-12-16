
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public record CommandEndpointDefinition<T> : EndpointDefinition<T>, ICommandEndpointDefinition
{
    public Func<object, Task<CommandResult>> ExecuteCommand { get;  set; } = _ => Task.FromResult(CommandResult.CompletedSuccessfully);
    public List<CommandResultPolicy> CommandResultPolicies { get;  set; } = [];
}