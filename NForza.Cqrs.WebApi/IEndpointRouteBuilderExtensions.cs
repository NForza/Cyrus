using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NForza.Cqrs.WebApi.Policies;

namespace NForza.Cqrs.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCqrs(this IEndpointRouteBuilder endpoints)
        => endpoints.MapQueries().MapCommands();

    public static IEndpointRouteBuilder MapQueries(this IEndpointRouteBuilder endpoints)
    {
        var queryEndpoints = endpoints.ServiceProvider.GetServices<QueryEndpointDefinition>();
        var queryHandlerDictionary = endpoints.ServiceProvider.GetRequiredService<QueryHandlerDictionary>();

        foreach (var queryEndpoint in queryEndpoints)
        {
            var resultType = queryHandlerDictionary.GetQueryReturnType(queryEndpoint.QueryType);
            MapQuery(endpoints, queryEndpoint, resultType);
        }
        return endpoints; ;
    }

    internal static RouteHandlerBuilder MapQuery(this IEndpointRouteBuilder endpoints, QueryEndpointDefinition endpointDefinition, Type queryResultType) 
        => endpoints
            .MapGet(endpointDefinition.Path, async (HttpContext ctx, IServiceProvider serviceProvider, IQueryProcessor queryProcessor)
                =>
            {
                var queryObject = await CreateQueryObjectAsync(endpointDefinition, ctx);

                if (!EndpointCommandMappingExtensions.ValidateObject(endpointDefinition.EndpointType, queryObject, ctx.RequestServices, out var problem))
                    return Results.BadRequest(problem);

                var queryResult = queryProcessor.QueryInternal(queryObject, endpointDefinition.QueryType, CancellationToken.None);

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
            .WithOpenApi(operation =>
            {
                foreach (var param in FindAllParametersInRoute(endpointDefinition.Path))
                {
                    operation.Parameters.Add(
                        new OpenApiParameter()
                        {
                            Name = param,
                            In = ParameterLocation.Path,
                            Required = true,
                            Schema = new OpenApiSchema() { Type = "string" }
                        });
                }
                return operation;
            })
            .WithMetadata(
                new ProducesResponseTypeAttribute(queryResultType, 200));

    static IEnumerable<string> FindAllParametersInRoute(string route)
    {
        var rgx = new Regex(@"\{(?<parameter>\w+)\}");
        MatchCollection matches = rgx.Matches(route);
        return matches.Select(m => m.Groups["parameter"].Value);
    }

    private static IResult DefaultQueryPolicy(object? queryResult)
        => queryResult == null ? Results.NotFound() : Results.Ok(queryResult);

    private static Task<object> CreateQueryObjectAsync(QueryEndpointDefinition endpointDefinition, HttpContext ctx)
    {
        var queryInputMappingPolicy = endpointDefinition.InputMappingPolicyType != null ?
            (InputMappingPolicy)ctx.RequestServices.GetRequiredService(endpointDefinition.InputMappingPolicyType)
            : new DefaultQueryInputMappingPolicy(ctx);
        return queryInputMappingPolicy.MapInputAsync(endpointDefinition.EndpointType);
    }
}