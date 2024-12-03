using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public class AcceptedWhenSucceededPolicy : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result)
    {
        if (result.Succeeded)
        {
            return Results.Accepted();
        }
        return null;
    }
}
