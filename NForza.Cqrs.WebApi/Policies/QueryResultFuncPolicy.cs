using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi.Policies;

internal class QueryResultFuncPolicy(Func<object?, IResult> resultFunc) : QueryResultPolicy
{
    public override IResult? CreateResultFromQueryResult(object? result)
        => resultFunc(result);
}