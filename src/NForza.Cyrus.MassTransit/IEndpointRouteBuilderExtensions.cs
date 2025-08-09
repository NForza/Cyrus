using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.MassTransit;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAsyncApi(this IEndpointRouteBuilder endpoints)
    {
        IPublishTopology publishTopology = endpoints.ServiceProvider.GetRequiredService<IPublishTopology>();
        endpoints
            .MapGet("/asyncapi", () =>
            {
                ICyrusModel model = CyrusModel.Aggregate(endpoints.ServiceProvider);
                string yaml = model.AsAsyncApiYaml(publishTopology);
                return Results.Content(yaml, "text/yaml");
            })
            .ExcludeFromDescription();
        endpoints.MapGet("/swagger", () => Results.Redirect("/scalar/v1", permanent: true)).ExcludeFromDescription();
        endpoints.MapGet("/swagger/index.html", () => Results.Redirect("/scalar/v1", permanent: true)).ExcludeFromDescription();

        return endpoints;
    }
}