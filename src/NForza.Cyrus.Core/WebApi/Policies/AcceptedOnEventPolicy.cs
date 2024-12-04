using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public class AcceptedOnEventPolicy<T>(string? urlPattern = null) : CommandResultPolicy
{
    public string? UrlPattern { get; } = urlPattern;

    public override IResult? FromCommandResult(CommandResult result)
    {
        if (result.HasEvent<T>())
        {
            T @event = result.Events.OfType<T>().First();
            return Results.Accepted(ReplacePropertyValues(UrlPattern, @event));
        }
        return null;
    }

    private string? ReplacePropertyValues(string? urlPattern, T? @event)
    {
        foreach (var property in typeof(T).GetProperties())
        {
            urlPattern = urlPattern?.Replace($"{{{property.Name}}}", property.GetValue(@event)?.ToString() ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }
        return urlPattern;
    }
}