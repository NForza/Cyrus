using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.Pipeline;

public class ValidateCommandPipelineAction<TCommand>(Func<CommandExecutionContext, IEnumerable<string>> validationFunc, ICommandExecutionPipelineAction next) : ICommandExecutionPipelineAction
    where TCommand : class
{
    public async Task<IResult?> ExecuteAsync(CommandExecutionContext context)
    {
        var validationErrors = validationFunc(context);
        if (validationErrors.Any())
        {
            return Results.BadRequest(validationErrors);
        }
        return await next.ExecuteAsync(context);
    }
}
