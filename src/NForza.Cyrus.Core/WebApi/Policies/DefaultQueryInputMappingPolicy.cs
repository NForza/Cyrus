using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class DefaultQueryInputMappingPolicy(IHttpContextAccessor contextAccessor, IHttpContextObjectFactory queryFactory)
{
    public Task<object?> MapInputAsync(Type typeToCreate)
    {
        return Task.FromResult<object?>(null);
        //return Task.FromResult(queryFactory.CreateFromHttpContext(typeToCreate, contextAccessor.HttpContext!, null))!;
    }
}
