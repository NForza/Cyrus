using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public class HttpContextObjectFactory : IHttpContextObjectFactory
{
    Dictionary<Type, Func<HttpContext, object?, object>> objectFactories = new();

    public void Register<T>(Func<HttpContext, object?, T> factory)
    {
        objectFactories[typeof(T)] = (ctx, o) => factory(ctx, o is T q ? q : null);
    }

    public T CreateFromHttpContextWithBodyAndRouteParameters<T>(HttpContext ctx, T body)
    {
        var func = objectFactories[typeof(T)];
        return (T)func(ctx, body);
    }

    public bool HasRouteOrQueryValueFor(string propertyName, HttpContext ctx, Type targetType, out object? value)
    {
        value = null;
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
            value = converter.ConvertFrom(value);
            return true;
        }

        return false;
    }

    public T CreateFromHttpContextWithRouteParameters<T>(HttpContext ctx)
    {
        var func = objectFactories[typeof(T)];
        return (T)func(ctx, null);
    }
}