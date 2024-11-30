using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi
{
    public interface IQueryFactory
    {
        public object CreateFromHttpContext(Type queryType, HttpContext ctx);
    }
}
