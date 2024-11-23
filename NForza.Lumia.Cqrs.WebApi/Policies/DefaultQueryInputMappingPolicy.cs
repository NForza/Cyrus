using Microsoft.AspNetCore.Http;

namespace NForza.Lumia.Cqrs.WebApi.Policies;

public class DefaultQueryInputMappingPolicy(IHttpContextAccessor contextAccessor, IQueryFactory queryFactory) : InputMappingPolicy
{
    public override Task<object> MapInputAsync(Type typeToCreate)
    {
        return Task.FromResult(queryFactory.CreateFromHttpContext(typeToCreate, contextAccessor.HttpContext!));
    }
}
