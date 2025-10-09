using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.Cqrs;

public abstract class CommandConsumer<T>(IMessageBus bus) : IConsumer<T>
    where T : class
{
    public abstract Task Consume(ConsumeContext<T> context);

    protected void HandleCommandResult(ConsumeContext<T> context, object commandResult)
    {
        switch (commandResult)
        {
            case (Result result, IEnumerable<object> messages):
                bus.Publish(messages);
                context.Respond(result);
                break;

            case (Result result, object message):
                bus.Publish(message);
                context.Respond(result);
                break;

            case (object obj, IEnumerable<object> messages) when obj is not Result:
                bus.Publish(messages);
                context.Respond(obj);
                break;

            case object obj:
                context.Respond(obj);
                break;

            case null:
                break;
        }
    }
}