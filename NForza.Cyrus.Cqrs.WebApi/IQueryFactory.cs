using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.WebApi
{
    public interface IQueryFactory
    {
        public object CreateFromHttpContext(Type queryType, HttpContext ctx);
    }
}
