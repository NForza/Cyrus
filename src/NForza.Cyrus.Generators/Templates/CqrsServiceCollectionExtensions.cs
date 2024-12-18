using System;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.WebApi.Policies;
% Usings %

namespace NForza.Cyrus.Cqrs;

#nullable enable

public static class CyrusServiceCollectionExtensions
{
    public static IServiceCollection AddCyrus(this IServiceCollection services, Action<CyrusOptions>? options = null)
    {
        options?.Invoke(new CyrusOptions(services));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddSingleton<ICommandBus, LocalCommandBus>();
        services.AddScoped<DefaultCommandInputMappingPolicy>();
        % RegisterEventBus %
        services.AddSingleton(BuildCommandHandlerDictionary());
        services.AddSingleton(BuildQueryHandlerDictionary());
        services.AddSingleton(BuildEventHandlerDictionary());
        % RegisterHandlerTypes %
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

    public static EventHandlerDictionary BuildEventHandlerDictionary()
    {
        var handlers = new EventHandlerDictionary();
        % RegisterEventHandlers %
        return handlers;
    }
}