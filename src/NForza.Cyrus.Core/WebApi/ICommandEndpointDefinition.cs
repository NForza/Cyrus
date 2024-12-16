using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi
{
    public interface ICommandEndpointDefinition: IEndpointDefinition
    {
        List<CommandResultPolicy> CommandResultPolicies { get; }
        Func<object, Task<CommandResult>> ExecuteCommand { get; }
    }
}