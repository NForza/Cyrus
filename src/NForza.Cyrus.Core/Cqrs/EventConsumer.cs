using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class EventConsumer<T>(IServiceProvider services, Action<T> eventHandler) : IConsumer<T>
    where T : class
{
    public async Task Consume(ConsumeContext<T> context)
    {
        eventHandler(context.Message);
    }
}
