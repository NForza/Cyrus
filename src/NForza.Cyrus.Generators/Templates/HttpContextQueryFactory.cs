using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

#nullable enable

public class HttpContextCqrsFactory : ICqrsFactory
{
    Dictionary<Type, Func<HttpContext, object>> objectFactories = new();

    public HttpContextCqrsFactory()
    {
        % QueryFactoryMethod %
    }

    public object CreateFromHttpContext(Type queryType, HttpContext ctx)
    {
        var func = objectFactories[queryType];
        return func(ctx);
    }

    private object? GetPropertyValue(string propertyName, HttpContext ctx, Type targetType)
    {
        if (ctx.Request.RouteValues.TryGetValue(propertyName, out var value))
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            return converter.ConvertFrom(value);
        }
        return null;
    }
}
