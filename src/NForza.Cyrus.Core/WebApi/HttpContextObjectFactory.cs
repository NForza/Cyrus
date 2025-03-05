using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public class HttpContextObjectFactory : IHttpContextObjectFactory
{
    Dictionary<Type, Func<HttpContext, object>> objectFactories = new();

    public void Register<T>(Func<HttpContext, T> factory)
    {
        objectFactories[typeof(T)] = ctx => factory(ctx);
    }

    public T CreateFromHttpContext<T>(HttpContext ctx)
    {
        var func = objectFactories[typeof(T)];
        return (T)func(ctx);
    }

    public object CreateFromHttpContext(Type t, HttpContext ctx)
    {
        var func = objectFactories[t];
        return func(ctx);
    }


    public object? GetPropertyValue(string propertyName, HttpContext ctx, Type targetType)
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