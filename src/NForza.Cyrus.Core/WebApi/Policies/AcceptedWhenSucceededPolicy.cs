using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public class AcceptedWhenSucceededPolicy(string? location = null) : CommandResultPolicy
{
    public string? Location { get; } = location;

    public override IResult? FromCommandResult(CommandResult result)
    {
        if (result.Succeeded)
        {
            return Results.Accepted(Location);
        }
        return null;
    }
}
