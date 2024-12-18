using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi
{
    public interface ICommandEndpointDefinition : IEndpointDefinition
    {
        List<AugmentInputPolicy> AugmentInputPolicies { get; set; }
        List<CommandResultPolicy> CommandResultPolicies { get; set; }
        Func<object, Task<CommandResult>> ExecuteCommand { get; set; }
    }
}