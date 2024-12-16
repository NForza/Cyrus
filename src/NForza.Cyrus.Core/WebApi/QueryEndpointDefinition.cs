
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public record QueryEndpointDefinition<T> : EndpointDefinition<T>, IQueryEndpointDefinition
{
    public Type QueryType => typeof(T);
    public List<QueryResultPolicy> QueryResultPolicies { get; internal set; } = [];
}