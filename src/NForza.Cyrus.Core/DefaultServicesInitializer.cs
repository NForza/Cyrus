﻿using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;
using NForza.Cyrus;
using Microsoft.Extensions.DependencyModel;
using NForza.Cyrus.Abstractions.Model;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Json;

public class DefaultCyrusServices : ICyrusInitializer
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IHttpContextObjectFactory, HttpContextObjectFactory>();
        services.AddTransient<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.Scan(scan => scan
            .FromDependencyContext(DependencyContext.Default!)
            .AddClasses(classes => classes.AssignableTo<ICyrusModel>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }
}