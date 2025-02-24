using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public interface ICqrsFactory
{
    public object CreateFromHttpContext(Type queryType, HttpContext ctx);
}
