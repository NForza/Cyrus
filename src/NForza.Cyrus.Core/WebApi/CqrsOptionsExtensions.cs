using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Cyrus.WebApi;

public static class CyrusOptionsExtensions
{
    public static CyrusOptions AddCqrsEndpoints(this CyrusOptions options)
    {
        options.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        options.Services.AddSingleton(sp =>
        {
            IEnumerable<EndpointGroup> groups = sp.GetServices<EndpointGroup>();
            IEnumerable<EndpointDefinition> definitions = groups.SelectMany(g => g.EndpointDefinitions);
            return definitions.OfType<CommandEndpointDefinition>();
        });
        options.Services.AddSingleton(sp =>
        {
            IEnumerable<EndpointGroup> groups = sp.GetServices<EndpointGroup>();
            IEnumerable<EndpointDefinition> definitions = groups.SelectMany(g => g.EndpointDefinitions);
            return definitions.OfType<QueryEndpointDefinition>();
        });
        options.Services.AddHttpContextAccessor();
        options.Services.AddTransient<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();
        options.Services.AddTransient<DefaultQueryInputMappingPolicy>();

        return options;
    }

    public static CyrusOptions ConfigureJsonConverters(this CyrusOptions options)
    {
        return options;
    }
}
