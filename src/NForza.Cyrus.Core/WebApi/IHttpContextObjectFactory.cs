using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public interface IHttpContextObjectFactory
{
    public (T? obj, IEnumerable<string> validationErrors) CreateFromHttpContextWithBodyAndRouteParameters<TContract, T>(HttpContext ctx, TContract body);
    public (T? obj, IEnumerable<string> validationErrors) CreateFromHttpContextWithRouteParameters<TContract, T>(HttpContext ctx);
}
