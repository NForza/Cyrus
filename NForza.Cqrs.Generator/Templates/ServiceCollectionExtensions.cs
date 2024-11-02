using System;
using Microsoft.Extensions.DependencyInjection;
% Usings %

namespace NForza.Cqrs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services, Action<CqrsOptions>? options = null)
    {
        options?.Invoke(new CqrsOptions(services));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddSingleton<ICommandBus, LocalCommandBus>(); 
        % RegisterEventBus % 
        services.AddSingleton(BuildCommandHandlerDictionary());
        services.AddSingleton(BuildQueryHandlerDictionary());
        % RegisterCommandHandlerTypes %
        % RegisterQueryHandlerTypes %
        return services;
    }

    public static CommandHandlerDictionary BuildCommandHandlerDictionary()
    {
        var handlers = new CommandHandlerDictionary();
        % RegisterCommandHandlers %
        return handlers;
    }

    public static QueryHandlerDictionary BuildQueryHandlerDictionary()
    {
        var handlers = new QueryHandlerDictionary();
        % RegisterQueryHandlers %
        return handlers;
    }

}