using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.Cqrs.Pipeline;

namespace NForza.Cyrus.WebApi;

public static class CommandDispatch
{
    public static async Task<IResult?> ExecuteAsync<TCommandContract, TCommand>(
            WebApplication app,
            TCommandContract commandContract,
            Func<CommandExecutionContext, IEnumerable<string>> validationFunc,
            Func<ICommandDispatcher, IMessageBus, TCommand, Task<IResult>> dispatchFunction)
        where TCommandContract : class
        where TCommand : class
    {
        var commandExecutionContext = new CommandExecutionContext(commandContract, app.Services.CreateScope());

        var dispatchCommand = new DispatchCommandPipelineAction<TCommandContract, TCommand>(dispatchFunction);

        ValidateCommandPipelineAction<TCommand> validateCommand = new ValidateCommandPipelineAction<TCommand>(
                    validationFunc,
                    next: dispatchCommand);

        CreateCommandPipelineAction<TCommandContract, TCommand> createCommand = new CreateCommandPipelineAction<TCommandContract, TCommand>(
            next: validateCommand);

        return await createCommand.ExecuteAsync(commandExecutionContext);
    }
}
