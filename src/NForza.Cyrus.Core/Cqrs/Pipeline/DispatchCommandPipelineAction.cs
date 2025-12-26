using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs.Pipeline;

public class DispatchCommandPipelineAction<TCommandContract, TCommand>(Func<ICommandDispatcher, IMessageBus, TCommand, Task<IResult>> dispatchFunction) : ICommandExecutionPipelineAction
    where TCommandContract : class
    where TCommand : class
{
    async Task<IResult?> ICommandExecutionPipelineAction.ExecuteAsync(CommandExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Command);
        ICommandDispatcher dispatcher = context.Services.GetRequiredService<ICommandDispatcher>();
        IMessageBus messageBus = context.Services.GetRequiredService<IMessageBus>();
        return await dispatchFunction(dispatcher, messageBus, (TCommand) context.Command);
    }
}
