using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs.WebApi;

public static class CqrsOptionsExtensions
{
    public static CqrsOptions AddEndpoints(this CqrsOptions options)
    {
        options.Services.AddSingleton(sp =>
        {
            IEnumerable<EndpointGroup> groups = sp.GetServices<EndpointGroup>();
            IEnumerable<EndpointDefinition> definitions = groups.SelectMany(g => g.EndpointDefinitions);
            return definitions.OfType<CommandEndpointDefinition>();
        });
        return options;
    }
}
