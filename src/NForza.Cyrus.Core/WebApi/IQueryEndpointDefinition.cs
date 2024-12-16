using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi
{
    public interface IQueryEndpointDefinition  : IEndpointDefinition
    {
        List<QueryResultPolicy> QueryResultPolicies { get; }
        Type QueryType { get; }
    }
}