using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public class QueryEndpointBuilder<T>(IQueryEndpointDefinition endpointDefinition)
{
    public QueryEndpointBuilder<T> Get(string path)
    {
        endpointDefinition.Method = "GET";
        endpointDefinition.EndpointPath = path;
        return this;
    }

    public QueryEndpointBuilder<T> Tags(params string[] tags)
    {
        endpointDefinition.Tags = tags;
        return this;
    }

    public QueryEndpointBuilder<T> AddPolicy(QueryResultPolicy policy)
    {
        endpointDefinition.QueryResultPolicies.Add(policy);
        return this;
    }

    public QueryEndpointBuilder<T> MapInputUsing<TInputPolicy>()
        where TInputPolicy : InputMappingPolicy
    {
        if (endpointDefinition.InputMappingPolicyType != null)
        {
            throw new InvalidOperationException($"Input mapping policy already set for {endpointDefinition.EndpointType.FullName}");
        }
        endpointDefinition.InputMappingPolicyType = typeof(TInputPolicy);
        return this;
    }
}