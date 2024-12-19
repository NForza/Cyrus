using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCyrus(this IEndpointRouteBuilder endpoints, bool logConfig)
    {
        if (logConfig)
        {
            LogCyrusConfiguration(endpoints.ServiceProvider);
        }
        return endpoints.MapCyrus();
    }

    private static void LogCyrusConfiguration(IServiceProvider serviceProvider)
    {
        var sb = new StringBuilder();
        var logger = serviceProvider.GetRequiredService<ILogger<CyrusConfig>>();
        var queryHandlers = serviceProvider.GetRequiredService<QueryHandlerDictionary>();
        sb.AppendLine("Queries handled:");
        foreach (var queryHandler in queryHandlers)
        {
            sb.AppendLine($"- {queryHandler.Value.HandlerName}");
        }

        var commandHandlers = serviceProvider.GetRequiredService<CommandHandlerDictionary>();
        sb.AppendLine("Commands handled:");
        foreach (var commandHandler in commandHandlers)
        {
            sb.AppendLine($"- {commandHandler.Key.FullName}");
        }

        var eventHandlers = serviceProvider.GetRequiredService<EventHandlerDictionary>();
        sb.AppendLine("Events handled:");
        foreach (var eventTypeHandler in eventHandlers)
        {
            sb.AppendLine($"- {eventTypeHandler.Key.FullName} (#handlers: {eventTypeHandler.Value.Count})");
        }
        logger.LogInformation(sb.ToString());
    }

    public static IEndpointRouteBuilder MapCyrus(this IEndpointRouteBuilder endpoints)
        => endpoints.MapQueries().MapCommands();

    public static IEndpointRouteBuilder MapQueries(this IEndpointRouteBuilder endpoints)
    {
        var queryEndpoints = endpoints.ServiceProvider.GetServices<IQueryEndpointDefinition>();
        var queryHandlerDictionary = endpoints.ServiceProvider.GetRequiredService<QueryHandlerDictionary>();

        foreach (var queryEndpoint in queryEndpoints)
        {
            var resultType = queryHandlerDictionary.GetQueryReturnType(queryEndpoint.QueryType);
            MapQuery(endpoints, queryEndpoint, resultType);
        }
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