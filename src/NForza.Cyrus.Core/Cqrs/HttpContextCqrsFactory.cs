using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.WebApi;

namespace NForza.Cyrus.Cqrs;

#nullable enable

public class HttpContextCqrsFactory(WebCqrsFactoryDictionary webCqrsFactoryDictionary) : ICqrsFactory
{
    public object CreateFromHttpContext(Type queryType, HttpContext ctx)
    {
        
        return webCqrsFactoryDictionary.CreateFromHttpContext(queryType, ctx);
    }

    private object? GetPropertyValue(string propertyName, HttpContext ctx, Type targetType)
    {
        object? value = null;

        if (ctx.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
        {
            value = routeValue;
        }
        else if (ctx.Request.Query.TryGetValue(propertyName, out var queryValue))
        {
            value = queryValue.ToString();
        }

        if (value != null)
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            return converter.ConvertFrom(value);
        }

        return null;
    }
}
