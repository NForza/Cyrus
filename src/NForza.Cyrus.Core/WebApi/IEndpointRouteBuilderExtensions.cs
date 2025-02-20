using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Abstractions;
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
        var queryEndpoints = endpoints.ServiceProvider.GetServices<IQueryEndpointDefinition>();
        var queryHandlerDictionary = endpoints.ServiceProvider.GetRequiredService<QueryHandlerDictionary>();

        foreach (var queryEndpoint in queryEndpoints)
        {
            var resultType = queryHandlerDictionary.GetQueryReturnType(queryEndpoint.QueryType);
            MapQuery(endpoints, queryEndpoint, resultType);
        }

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

    internal static RouteHandlerBuilder MapQuery(this IEndpointRouteBuilder endpoints, IQueryEndpointDefinition endpointDefinition, Type queryResultType)
        => endpoints
            .MapGet(endpointDefinition.Path, async (HttpContext ctx, IServiceProvider serviceProvider, IQueryProcessor queryProcessor)
                =>
            {
                var queryObject = await CreateQueryObjectAsync(endpointDefinition, ctx);

                if (!EndpointCommandMappingExtensions.ValidateObject(endpointDefinition.EndpointType, queryObject, ctx.RequestServices, out var problem))
                    return Results.BadRequest(problem);

                var queryResult = await queryProcessor.QueryInternal(queryObject, endpointDefinition.QueryType, CancellationToken.None);

                var queryResultPolicies = endpointDefinition.QueryResultPolicies;
                foreach (var policy in queryResultPolicies)
                {
                    queryResult = policy.MapFromQueryResult(queryResult);
                    var result = policy.CreateResultFromQueryResult(queryResult);
                    if (result != null)
                        return result;
                }

                return DefaultQueryPolicy(queryResult);
            })
            .WithTags(endpointDefinition.Tags)
            .WithSwaggerParameters(endpointDefinition.Path)
            .WithMetadata(
                new ProducesResponseTypeAttribute(queryResultType, 200));

    private static IResult DefaultQueryPolicy(object? queryResult)
        => queryResult == null ? Results.NotFound() : Results.Ok(queryResult);

    private static Task<object> CreateQueryObjectAsync(IQueryEndpointDefinition endpointDefinition, HttpContext ctx)
    {
        var queryInputMappingPolicy = endpointDefinition.InputMappingPolicyType != null ?
            (InputMappingPolicy)ctx.RequestServices.GetRequiredService(endpointDefinition.InputMappingPolicyType)
            : ctx.RequestServices.GetRequiredService<DefaultQueryInputMappingPolicy>();
        return queryInputMappingPolicy.MapInputAsync(endpointDefinition.EndpointType);
    }
}