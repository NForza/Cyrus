
using NForza.Cyrus.Cqrs.WebApi.Policies;

namespace NForza.Cyrus.Cqrs.WebApi;

public record QueryEndpointDefinition(Type QueryType) : EndpointDefinition(QueryType)
{
    public List<QueryResultPolicy> QueryResultPolicies { get; internal set; } = [];
}