using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public abstract class CommandResultPolicy
{
    public abstract IResult? FromCommandResult(CommandResult result);
}