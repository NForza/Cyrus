using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.WebApi.Policies;

/// <summary>
/// Returns OK http response when data is null
/// </summary>
public class OkWhenNullPolicy : QueryResultPolicy
{
    public override IResult? CreateResultFromQueryResult(object? result)
        => result == null ? Results.Ok() : null;
}