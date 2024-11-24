using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.WebApi.Policies;

public abstract class CommandResultPolicy
{
    public abstract IResult? FromCommandResult(CommandResult result);
}