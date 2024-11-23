using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NForza.Lumia.Cqrs.WebApi.Policies;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Lumia.Cqrs.WebApi;

public static class CqrsOptionsExtensions
{
    public static CqrsOptions AddCqrsEndpoints(this CqrsOptions options)
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

    public static CqrsOptions ConfigureJsonConverters(this CqrsOptions options)
    {
        return options;
    }
}
