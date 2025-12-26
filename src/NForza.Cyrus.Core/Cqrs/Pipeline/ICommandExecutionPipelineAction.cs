using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.Pipeline;

public interface ICommandExecutionPipelineAction
{
    Task<IResult?> ExecuteAsync(CommandExecutionContext context);
}
