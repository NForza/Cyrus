using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs;

public interface ICqrsFactory
{
    public object CreateFromHttpContext(Type queryType, HttpContext ctx);
}
