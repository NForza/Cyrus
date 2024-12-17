using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public static class QueryEndpointBuilderExtensions
{
    public static QueryEndpointBuilder<T> OkWhenNull<T>(this QueryEndpointBuilder<T> builder)
        => builder.AddPolicy(new OkWhenNullPolicy());

    public static QueryEndpointBuilder<T> Map<T, TInput>(this QueryEndpointBuilder<T> builder, Func<TInput, object?> mapFunc)
        where TInput : class
        => builder.AddPolicy(new MapPolicy<TInput>(mapFunc));

    public static QueryEndpointBuilder<T> Result<T>(this QueryEndpointBuilder<T> builder, Func<object?, IResult> resultFunc)
        => builder.AddPolicy(new QueryResultFuncPolicy(resultFunc));

}
