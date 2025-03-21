using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public interface IHttpContextObjectFactory
{
    public T CreateFromHttpContextWithBodyAndRouteParameters<TContract, T>(HttpContext ctx, TContract body);
    public T CreateFromHttpContextWithRouteParameters<TContract, T>(HttpContext ctx);
}
