using Microsoft.AspNetCore.Routing;

namespace NForza.Cqrs.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCqrs(this IEndpointRouteBuilder endpoints)
        => endpoints.MapQueries().MapCommands();

    public static IEndpointRouteBuilder MapQueries(this IEndpointRouteBuilder endpoints)
    {
        //var queryDefinitions = AppDomain.CurrentDomain
        //    .GetQueryEndpointDefinitions()
        //    .Where(t => filter == null || filter(t.EndpointType))
        //    .ToList();

        //queryDefinitions.ForEach(queryDefinition => MapQuery(endpoints, queryDefinition));

        return endpoints;
    }

    //internal static RouteHandlerBuilder MapQuery(this IEndpointRouteBuilder endpoints, QueryEndpointDefinition endpointDefinition) => endpoints.MapGet(endpointDefinition.Path, async (HttpContext ctx, IServiceProvider serviceProvider, IQueryProcessor queryProcessor)
    //    =>
    //{
    //    var queryObject = await CreateQueryObjectAsync( endpointDefinition, ctx);

    //    if (!EndpointCommandMappingExtensions.ValidateObject(endpointDefinition.EndpointType, queryObject, ctx.RequestServices, out var problem))
    //        return Results.BadRequest(problem);

    //    var queryResultType = FindQueryResultType(endpointDefinition.EndpointType);
    //    var genericMethodInfo = queryProcessor.GetType().GetMethod("Query");
    //    var queryMethodInfo = genericMethodInfo!.MakeGenericMethod(queryResultType);
    //    var resultTask = queryMethodInfo.Invoke(queryProcessor, [queryObject, CancellationToken.None])!;
    //    await (Task)resultTask;
    //    var queryResult = resultTask.GetType().GetProperty("Result")!.GetValue(resultTask);

    //    var queryResultPolicies = endpointDefinition.QueryResultPolicies;
    //    foreach (var policy in queryResultPolicies)
    //    {
    //        queryResult = policy.MapFromQueryResult(queryResult);
    //        var result = policy.CreateResultFromQueryResult(queryResult);
    //        if (result != null)
    //            return result;
    //    }

    //    return DefaultQueryPolicy(queryResult);
    //})
    //.WithTags(endpointDefinition.Tags)
    //.WithOpenApi(operation =>
    //{
    //    foreach (var param in FindAllParametersInRoute(endpointDefinition.Path))
    //    {
    //        operation.Parameters.Add(
    //            new OpenApiParameter()
    //            {
    //                Name = param,
    //                In = ParameterLocation.Path,
    //                Required = true,
    //                Schema = new OpenApiSchema() { Type = "string" }
    //            });
    //    }
    //    return operation;
    //})
    //.WithMetadata(
    //    new ProducesResponseTypeAttribute(FindQueryResultType(endpointDefinition.EndpointType), 200));

    //static IEnumerable<string> FindAllParametersInRoute(string route)
    //{
    //    var rgx = new Regex(@"\{(?<parameter>\w+)\}");
    //    MatchCollection matches = rgx.Matches(route);
    //    return matches.Select(m => m.Groups["parameter"].Value);
    //}

    //static Type FindQueryResultType(Type queryType)
    //{
    //    var queryDefinition = queryType.GetInterfaces().First(intf =>
    //        intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IQuery<>));
    //    var queryResultType = queryDefinition.GetGenericArguments().First();
    //    return queryResultType;
    //}

    //private static IResult DefaultQueryPolicy(object? queryResult)
    //    => queryResult == null ? Results.NotFound() : Results.Ok(queryResult);

    //private static Task<object> CreateQueryObjectAsync(QueryEndpointDefinition endpointDefinition, HttpContext ctx)
    //{
    //    var queryInputMappingPolicy = endpointDefinition.InputMappingPolicyType != null ?
    //        (InputMappingPolicy)ctx.RequestServices.GetRequiredService(endpointDefinition.InputMappingPolicyType)
    //        : ctx.RequestServices.GetRequiredService<DefaultQueryInputMappingPolicy>();
    //    return queryInputMappingPolicy.MapInputAsync(endpointDefinition.EndpointType);
    //}
}