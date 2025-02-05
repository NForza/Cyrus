﻿using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi.Policies;

public class DefaultQueryInputMappingPolicy(IHttpContextAccessor contextAccessor, ICqrsFactory queryFactory) : InputMappingPolicy
{
    public override Task<object?> MapInputAsync(Type typeToCreate)
    {
        return Task.FromResult(queryFactory.CreateFromHttpContext(typeToCreate, contextAccessor.HttpContext!))!;
    }
}
