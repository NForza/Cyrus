using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public class HttpContextObjectFactory : IHttpContextObjectFactory
{
    private MethodInfo registerMethod = typeof(HttpContextObjectFactory).GetMethod(nameof(Register), BindingFlags.NonPublic | BindingFlags.Instance)!;
    public HttpContextObjectFactory(IEnumerable<ObjectFactoryRegistration> objectFactoryRegistrations)
    {
        foreach(var registration in objectFactoryRegistrations)
        {
            registerMethod.MakeGenericMethod(registration.Type).Invoke(this, [registration.FactoryMethod]);
        }   
    }

    Dictionary<Type, Func<HttpContext, object?, object>> objectFactories = new();

    private void Register<T>(Func<HttpContext, object?, object> factory)
    {
        objectFactories[typeof(T)] = (ctx, o) => factory(ctx, o is T q ? q : null);
    }

    public T CreateFromHttpContextWithBodyAndRouteParameters<TContract, T>(HttpContext ctx, TContract body)
    {
        var func = objectFactories[typeof(TContract)];
        return (T)func(ctx, body);
    }

    public T CreateFromHttpContextWithRouteParameters<TContract, T>(HttpContext ctx)
    {
        var func = objectFactories[typeof(TContract)];
        return (T)func(ctx, null);
    }
}