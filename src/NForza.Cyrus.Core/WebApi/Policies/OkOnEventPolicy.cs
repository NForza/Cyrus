using Microsoft.AspNetCore.Http;

using NForza.Cyrus.Cqrs;
namespace NForza.Cyrus.WebApi.Policies;

public class OkOnEventPolicy<T> : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result)
    {
        if (result.HasEvent<T>())
        {
            T @event = result.Events.OfType<T>().First();
            return Results.Ok(@event);
        }
        return null;
    }
}