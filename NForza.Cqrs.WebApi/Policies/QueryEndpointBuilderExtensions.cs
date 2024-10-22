using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi.Policies;

public static class QueryEndpointBuilderExtensions
{
    public static QueryEndpointBuilder OkWhenNull(this QueryEndpointBuilder builder)
        => builder.AddPolicy(new OkWhenNullPolicy());

    public static QueryEndpointBuilder Map<T>(this QueryEndpointBuilder builder, Func<T, object?> mapFunc)
        where T : class
        => builder.AddPolicy(new MapPolicy<T>(mapFunc));

    public static QueryEndpointBuilder Result(this QueryEndpointBuilder builder, Func<object?, IResult> resultFunc)
        => builder.AddPolicy(new QueryResultFuncPolicy(resultFunc));

}
