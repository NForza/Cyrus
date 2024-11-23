namespace NForza.Lumia.Cqrs.WebApi;

public class EndpointGroup(string tag, string path)
{
    private readonly Dictionary<Type, EndpointDefinition> endpoints = [];

    public EndpointGroup(string tag) : this(tag, tag.ToLowerInvariant())
    {
    }

    public string[] Tags { get; private set; } = [tag];

    internal IEnumerable<EndpointDefinition> EndpointDefinitions => endpoints.Values;

    public CommandEndpointBuilder CommandEndpoint<T>()
    {
        CommandEndpointDefinition endpointDefinition = new(typeof(T));
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
        QueryEndpointDefinition endpointDefinition = new(typeof(T));
        if (!endpoints.TryAdd(typeof(T), endpointDefinition))
        {
            throw new InvalidOperationException($"Endpoint for {typeof(T).Name} already exists.");
        }
        endpointDefinition.Tags = Tags;
        return new QueryEndpointBuilder(endpointDefinition);
    }
}