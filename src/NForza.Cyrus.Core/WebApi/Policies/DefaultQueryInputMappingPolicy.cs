using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.Policies;

public class DefaultQueryInputMappingPolicy(IHttpContextAccessor contextAccessor, ICqrsFactory queryFactory) 
{
    public Task<object?> MapInputAsync(Type typeToCreate)
    {
        return Task.FromResult(queryFactory.CreateFromHttpContext(typeToCreate, contextAccessor.HttpContext!))!;
    }
}
