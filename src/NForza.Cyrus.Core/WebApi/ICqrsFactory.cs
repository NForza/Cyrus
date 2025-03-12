using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public interface IHttpContextObjectFactory
{
    public T CreateFromHttpContextWithBodyAndRouteParameters<T>(HttpContext ctx, T body);
    public T CreateFromHttpContextWithRouteParameters<T>(HttpContext ctx);
   // public object CreateFromHttpContext(Type t, HttpContext ctx, object? obj);
}
