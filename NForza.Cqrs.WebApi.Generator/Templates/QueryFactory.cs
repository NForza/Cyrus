using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi;

public class QueryFactory : IQueryFactory
{
    Dictionary<Type, Func<object>> objectFactories = new();

    public QueryFactory()
    {
        % QueryFactoryMethod %
    }

    public object CreateFromHttpContext(Type queryType, HttpContext ctx)
    {
        var func = objectFactories[queryType]; 
        return func();
    }
}
