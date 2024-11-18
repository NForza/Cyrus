using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi
{
    public interface IQueryFactory
    {
        public object CreateFromHttpContext(Type queryType, HttpContext ctx);
    }
}
