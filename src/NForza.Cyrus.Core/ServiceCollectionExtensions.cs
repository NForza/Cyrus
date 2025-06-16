using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCyrus(this IServiceCollection services)
    {
        var registrarTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsFrameworkAssembly())
            .SelectMany(a => a.GetTypes())
            .Distinct()
            .Where(t => typeof(ICyrusInitializer).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in registrarTypes)
        {
            var registrar = (ICyrusInitializer)Activator.CreateInstance(type)!;
            registrar.RegisterServices(services);
        }
        return services;
    }
}
