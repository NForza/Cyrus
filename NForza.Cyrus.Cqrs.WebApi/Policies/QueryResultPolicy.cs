using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.Cqrs.WebApi.Policies;

public class QueryResultPolicy
{
    public virtual object? MapFromQueryResult(object? result) => result;
    public virtual IResult? CreateResultFromQueryResult(object? result) => null;
}