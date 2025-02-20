using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.SignalR;
using NForza.Cyrus.WebApi.Policies;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCyrus(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapQueries().MapCommands().MapModel().MapSignalR();
    }

    public static IEndpointRouteBuilder MapQueries(this IEndpointRouteBuilder endpoints)
    {
        var queryHandlerDictionary = endpoints.ServiceProvider.GetRequiredService<QueryHandlerDictionary>();

        foreach (var (queryType, queryHandler) in queryHandlerDictionary)
        {
            endpoints
                .MapGet(queryHandler.Route, async (HttpContext ctx, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ICqrsFactory cqrsFactory, IQueryProcessor queryProcessor) =>
                    {
                        var queryObject = await new DefaultQueryInputMappingPolicy(httpContextAccessor, cqrsFactory).MapInputAsync(queryType);

                        if (!EndpointCommandMappingExtensions.ValidateObject(queryType, queryObject, ctx.RequestServices, out var problem))
                            return Results.BadRequest(problem);

                        var queryResult = await queryProcessor.QueryInternal(queryObject, queryType, CancellationToken.None);
                        return DefaultQueryPolicy(queryResult);
                    })
                 .WithSwaggerParameters(queryHandler.Route);
        }
        return endpoints;
    }
    public static IEndpointRouteBuilder MapModel(this IEndpointRouteBuilder endpoints)
    {
        ICyrusModel model = CyrusModel.Aggregate(endpoints.ServiceProvider);
        endpoints.MapGet("/model", () =>
        {
            string json = model.AsJson();
            return Results.Content(json, "application/json");
        });
        return endpoints;
    }

    public static IEndpointRouteBuilder MapSignalR(this IEndpointRouteBuilder endpoints)
    {
        var hubs = endpoints.ServiceProvider.GetService<SignalRHubDictionary>();
        hubs?.ToList().ForEach(hub => hub.Value(endpoints));
        return endpoints;
    }

    private static IResult DefaultQueryPolicy(object? queryResult)
        => queryResult == null ? Results.NotFound() : Results.Ok(queryResult);
}