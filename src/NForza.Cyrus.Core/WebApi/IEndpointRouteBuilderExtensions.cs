using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.WebApi;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCyrus(this WebApplication endpoints, ILogger? logger = null)
    {
        endpoints = (WebApplication) endpoints.UseJsonExceptionHandler();
        IEnumerable<Assembly> assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsFrameworkAssembly())
            .ToList();

        IEnumerable<Type> typesToScan = assembliesToScan
            .SelectMany(assembly => assembly.GetTypes())
            .ToList();

        IEnumerable<Type> typesToCreate = typesToScan
            .Where(type => type.IsAssignableTo(typeof(ICyrusWebStartup)) && !type.IsInterface && !type.IsAbstract)
            .ToList();

        IEnumerable<ICyrusWebStartup> startupObjects = typesToCreate
            .Select(type => (ICyrusWebStartup)Activator.CreateInstance(type)!)
            .ToList();

        foreach (var startup in startupObjects)
        {
            logger?.LogInformation($"Adding startup {startup.GetType().Name}");
            startup.AddStartup(endpoints);
        }
        return endpoints.MapModel();
    }

    public static IEndpointRouteBuilder MapModel(this IEndpointRouteBuilder endpoints)
    {
        ICyrusModel model = CyrusModel.Aggregate(endpoints.ServiceProvider);
        endpoints
            .MapGet("/model", () =>
            {
                string json = model.AsJson();
                return Results.Content(json, "application/json");
            })
            .ExcludeFromDescription();
        endpoints.MapGet("/swagger", () => Results.Redirect("/scalar/v1", permanent: true)).ExcludeFromDescription();
        endpoints.MapGet("/swagger/index.html", () => Results.Redirect("/scalar/v1", permanent: true)).ExcludeFromDescription();

        return endpoints;
    }
}