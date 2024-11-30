using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

internal class ResultFuncPolicy(Func<CommandResult, IResult> resultFunc) : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result)
        => resultFunc(result);
}