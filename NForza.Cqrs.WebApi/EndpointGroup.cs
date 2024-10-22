namespace NForza.Cqrs.WebApi;

public class EndpointGroup
{
    private Dictionary<Type, EndpointDefinition> endpoints = [];
    private readonly string path;

    public EndpointGroup(string tag) : this(tag, tag.ToLowerInvariant())
    {
    }

    public EndpointGroup(string tag, string path)
    {
        this.Tags = [tag];
        this.path = path;
    }

    public string[] Tags { get; private set; } = [];

    internal IEnumerable<EndpointDefinition> EndpointDefinitions => endpoints.Values;

    public CommandEndpointBuilder CommandEndpoint<T>()
    {
        CommandEndpointDefinition endpointDefinition = new CommandEndpointDefinition(typeof(T));
        if (!endpoints.TryAdd(typeof(T), endpointDefinition))
        {
            throw new InvalidOperationException($"Endpoint for {typeof(T).Name} already exists.");
        }
        endpointDefinition.Tags = Tags;
        endpointDefinition.GroupPath = path;
        return new CommandEndpointBuilder(endpointDefinition);
    }

    public QueryEndpointBuilder QueryEndpoint<T>()
    {
        QueryEndpointDefinition endpointDefinition = new QueryEndpointDefinition(typeof(T));
        if (!endpoints.TryAdd(typeof(T), endpointDefinition))
        {
            throw new InvalidOperationException($"Endpoint for {typeof(T).Name} already exists.");
        }
        endpointDefinition.Tags = Tags;
        return new QueryEndpointBuilder(endpointDefinition);
    }
}