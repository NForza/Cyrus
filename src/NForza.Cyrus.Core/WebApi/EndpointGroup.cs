namespace NForza.Cyrus.WebApi;

public class EndpointGroup(string tag, string path)
{
    private readonly Dictionary<Type, IEndpointDefinition> endpoints = [];

    public EndpointGroup(string tag) : this(tag, tag.ToLowerInvariant())
    {
    }

    public string[] Tags { get; private set; } = [tag];

    internal IEnumerable<IEndpointDefinition> EndpointDefinitions => endpoints.Values;

    public CommandEndpointBuilder<T> CommandEndpoint<T>()
    {
        ICommandEndpointDefinition endpointDefinition = new CommandEndpointDefinition<T>();
        if (!endpoints.TryAdd(typeof(T), endpointDefinition))
        {
            throw new InvalidOperationException($"Endpoint for {typeof(T).Name} already exists.");
        }
        endpointDefinition.Tags = Tags;
        endpointDefinition.GroupPath = path;
        return new CommandEndpointBuilder<T>(endpointDefinition);
    }
}