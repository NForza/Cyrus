using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.SignalR;
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
            IEnumerable<IEndpointDefinition> definitions = groups.SelectMany(g => g.EndpointDefinitions);
            return definitions.OfType<ICommandEndpointDefinition>();
        });
        options.Services.AddSingleton(sp =>
        {
            IEnumerable<EndpointGroup> groups = sp.GetServices<EndpointGroup>();
            IEnumerable<IEndpointDefinition> definitions = groups.SelectMany(g => g.EndpointDefinitions);
            return definitions.OfType<IQueryEndpointDefinition>();
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
