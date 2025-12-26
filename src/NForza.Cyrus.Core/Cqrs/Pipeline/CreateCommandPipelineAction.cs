using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.WebApi;

namespace NForza.Cyrus.Cqrs.Pipeline;

public class CreateCommandPipelineAction<TCommandContract, TCommand>(ICommandExecutionPipelineAction? next) : ICommandExecutionPipelineAction
    where TCommandContract : class
    where TCommand : class
{
    public async Task<IResult?> ExecuteAsync(CommandExecutionContext context)
    {
        var services = context.Services;
        var objectFactory = services.GetRequiredService<IHttpContextObjectFactory>();
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var (cmd, validationErrors) = objectFactory
            .CreateFromHttpContextWithBodyAndRouteParameters<TCommandContract, TCommand>(httpContextAccessor.HttpContext!, (TCommandContract) context.CommandContract);
        if (validationErrors.Any())
            return Results.BadRequest(validationErrors);
        context.Command = cmd;
        IResult? result = next == null ? Results.Problem() : await next!.ExecuteAsync(context);
        return result;
    }
}
