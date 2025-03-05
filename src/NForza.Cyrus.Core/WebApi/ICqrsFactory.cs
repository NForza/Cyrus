using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public interface IHttpContextObjectFactory
{
    public T CreateFromHttpContext<T>(HttpContext ctx);
    public object CreateFromHttpContext(Type t, HttpContext ctx);
}
