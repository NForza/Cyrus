
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public record ObjectFactoryRegistration(Type Type, Func<HttpContext, object?, object> FactoryMethod);
