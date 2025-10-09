using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCyrus(this IServiceCollection services, ILogger<ICyrusInitializer>? logger = null)
    {
        var assemblyNames = Assembly.GetEntryAssembly()!.GetReferencedAssemblies().Where(a => !a.IsFrameworkAssembly());
        EnsureAssembliesAreLoaded(assemblyNames);

        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsFrameworkAssembly())
            .ToList();

        logger?.LogDebug("Scanning {Count} assemblies for ICyrusInitializer implementations: {Assemblies}", assembliesToScan.Count(), string.Join(", ", assembliesToScan.Select(a => a.GetName().Name)));
        var registrarTypes = assembliesToScan.SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICyrusInitializer).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in registrarTypes)
        {
            var registrar = (ICyrusInitializer)Activator.CreateInstance(type)!;
            registrar.RegisterServices(services);
        }
        return services;
    }

    private static void EnsureAssembliesAreLoaded(IEnumerable<AssemblyName>? assemblyNames)
    {
        foreach (var assemblyName in assemblyNames ?? Enumerable.Empty<AssemblyName>())
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                var referencedAssemblyNames = assembly.GetReferencedAssemblies().Where(a => !a.IsFrameworkAssembly());
                EnsureAssembliesAreLoaded(referencedAssemblyNames);
            }
            catch
            {
                // Ignore load failures
            }
        }
    }
}
