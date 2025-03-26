using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCyrus(this WebApplication endpoints, ILogger? logger = null)
    {
        IEnumerable<ICyrusWebStartup> startups = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICyrusWebStartup).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(type => (ICyrusWebStartup)Activator.CreateInstance(type)!);

        foreach (var startup in startups)
        {
            logger?.LogInformation($"Adding startup {startup.GetType().Name}");
            startup.AddStartup(endpoints);
        }
        return endpoints.MapModel();
    }

    public static IEndpointRouteBuilder MapModel(this IEndpointRouteBuilder endpoints)
    {
        ICyrusModel model = CyrusModel.Aggregate(endpoints.ServiceProvider);
        endpoints
            .MapGet("/model", () =>
            {
                string json = model.AsJson();
                return Results.Content(json, "application/json");
            })
            .ExcludeFromDescription();
        return endpoints;
    }
}