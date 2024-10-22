
using NForza.Cqrs.WebApi.Policies;

namespace NForza.Cqrs.WebApi;

public record QueryEndpointDefinition(Type QueryType) : EndpointDefinition(QueryType)
{
    public List<QueryResultPolicy> QueryResultPolicies { get; internal set; } = [];
}