
using NForza.Lumia.Cqrs.WebApi.Policies;

namespace NForza.Lumia.Cqrs.WebApi;

public record QueryEndpointDefinition(Type QueryType) : EndpointDefinition(QueryType)
{
    public List<QueryResultPolicy> QueryResultPolicies { get; internal set; } = [];
}