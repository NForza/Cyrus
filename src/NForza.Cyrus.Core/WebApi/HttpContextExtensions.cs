using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace NForza.Cyrus.WebApi;

public static class HttpContextExtensions
{
    public static bool HasRouteOrQueryValueFor(this HttpContext ctx, string propertyName, Type targetType, out object? value)
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
}
