﻿// <auto-generated/>
using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services, Action<CqrsOptions>? options = null)
    {
        options?.Invoke(new CqrsOptions(services));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddSingleton<ICommandBus, LocalCommandBus>(); 
        services.AddSingleton<IEventBus, EventBus>(); 
        services.AddSingleton(BuildCommandHandlerDictionary());
        services.AddSingleton(BuildQueryHandlerDictionary());
        % RegisterTypes %
        return services;
    }

    public static CommandHandlerDictionary BuildCommandHandlerDictionary()
    {
        var handlers = new CommandHandlerDictionary();
        % RegisterCommandHandlers %
        return handlers;
    }

    public static QueryProcessorDictionary BuildQueryHandlerDictionary()
    {
        var handlers = new QueryProcessorHandlerDictionary();
        % RegisterQueryHandlers %
        return handlers;
    }

}