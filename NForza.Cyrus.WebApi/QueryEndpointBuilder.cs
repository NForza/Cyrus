using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public class QueryEndpointBuilder(QueryEndpointDefinition endpointDefinition)
{
    public QueryEndpointBuilder Get(string path)
    {
        endpointDefinition.Method = "GET";
        endpointDefinition.EndpointPath = path;
        return this;
    }

    public QueryEndpointBuilder Tags(params string[] tags)
    {
        endpointDefinition.Tags = tags;
        return this;
    }

    public QueryEndpointBuilder AddPolicy(QueryResultPolicy policy)
    {
        endpointDefinition.QueryResultPolicies.Add(policy);
        return this;
    }

    public QueryEndpointBuilder MapInputUsing<T>()
        where T : InputMappingPolicy
    {
        if (endpointDefinition.InputMappingPolicyType != null)
        {
            throw new InvalidOperationException($"Input mapping policy already set for {endpointDefinition.EndpointType.FullName}");
        }
        endpointDefinition.InputMappingPolicyType = typeof(T);
        return this;
    }
}