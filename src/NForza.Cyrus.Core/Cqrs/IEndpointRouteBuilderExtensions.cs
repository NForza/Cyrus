using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.Cqrs;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAsyncApi(this IEndpointRouteBuilder endpoints)
    {
        IBus bus = endpoints.ServiceProvider.GetRequiredService<IBus>();
        endpoints
            .MapGet("/asyncapi", () =>
            {
                ICyrusModel model = CyrusModel.Aggregate(endpoints.ServiceProvider);
                string yaml = model.AsAsyncApiYaml(bus);
                return Results.Content(yaml, "text/yaml");
            })
            .ExcludeFromDescription();
        return endpoints;
    }
}