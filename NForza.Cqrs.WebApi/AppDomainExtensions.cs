namespace NForza.Cqrs.WebApi;

public static class AppDomainExtensions
{
    private static IEnumerable<EndpointDefinition> GetEndpointDefinitions(this AppDomain appDomain)
    {
        var endpointGroups = appDomain.GetAssemblies()
          .Where(a => !a.IsFrameworkAssembly())
          .SelectMany(s => s.GetTypes())
          .Where(t => t.IsAssignableTo(typeof(EndpointGroup)))
          .Select(t => (EndpointGroup)Activator.CreateInstance(t)!);
        var allEndpoints = endpointGroups.SelectMany(g => g.EndpointDefinitions);
        ValidateEndpoints(allEndpoints);
        return allEndpoints;
    }

    private static void ValidateEndpoints(IEnumerable<EndpointDefinition> allEndpoints)
    {
        allEndpoints.ToList().ForEach(e =>
        {
            if (string.IsNullOrWhiteSpace(e.Method))
            {
                throw new InvalidOperationException($"Method not set for endpoint {e.EndpointType.Name}.");
            }
            if (string.IsNullOrWhiteSpace(e.Path))
            {
                throw new InvalidOperationException($"Path not set for endpoint {e.EndpointType.Name}.");
            }
        });
    }

    public static IEnumerable<CommandEndpointDefinition> GetCommandEndpointDefinitions(this AppDomain appDomain)
        => appDomain.GetEndpointDefinitions().OfType<CommandEndpointDefinition>();

    public static IEnumerable<QueryEndpointDefinition> GetQueryEndpointDefinitions(this AppDomain appDomain)
        => appDomain.GetEndpointDefinitions().OfType<QueryEndpointDefinition>();
}
