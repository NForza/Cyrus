using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.MassTransit;

public class EventConsumer<T>(EventHandlerDictionary eventHandlerDictionary, IServiceScopeFactory serviceScopeFactory) : IConsumer<T>
    where T : class
{
    public Task Consume(ConsumeContext<T> context)
    {
        var handlers = eventHandlerDictionary.GetEventHandlers<T>();
        var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        foreach (var handler in handlers)
        {
            handler(serviceProvider, context.Message!);
        }
        return Task.CompletedTask;
    }
}
