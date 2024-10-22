using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi.Policies;

internal class ResultFuncPolicy(Func<CommandResult, IResult> resultFunc) : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result)
        => resultFunc(result);
}