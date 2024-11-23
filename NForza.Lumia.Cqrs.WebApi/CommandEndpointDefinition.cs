
using NForza.Lumia.Cqrs.WebApi.Policies;

namespace NForza.Lumia.Cqrs.WebApi;

public record CommandEndpointDefinition(Type EndpointType) : EndpointDefinition(EndpointType)
{
    public Func<object, Task<CommandResult>> ExecuteCommand { get; internal set; } = _ => Task.FromResult(CommandResult.CompletedSuccessfully);
    public List<CommandResultPolicy> CommandResultPolicies { get; internal set; } = [];
}