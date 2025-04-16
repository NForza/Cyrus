using System;
using System.Collections.Generic;
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

    Dictionary<Type, Func<HttpContext, object?, (object, IEnumerable<string>)>> objectFactories = new();

    private void Register<T>(Func<HttpContext, object?, (object, IEnumerable<string>)> factory)
    {
        objectFactories[typeof(T)] = (ctx, o) => factory(ctx, o is T q ? q : null);
    }

    public (T? obj, IEnumerable<string> validationErrors) CreateFromHttpContextWithBodyAndRouteParameters<TContract, T>(HttpContext ctx, TContract body)
    {
        var func = objectFactories[typeof(TContract)];
        var result = func(ctx, body);
        if (result.Item1 != null)
            return ((T?)result.Item1, []);
        return (default(T), result.Item2);
    }

    public (T obj, IEnumerable<string> validationErrors) CreateFromHttpContextWithRouteParameters<TContract, T>(HttpContext ctx)
    {
        var func = objectFactories[typeof(TContract)];
        var result = func(ctx, null);
        return ((T)result.Item1, result.Item2);
    }
}