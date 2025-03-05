using System.Collections;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public static class QueryResultAdapter
{
    public static IResult FromObject(object obj)
    {
        if (obj == null)
        {
            return Results.NotFound();
        }
        if (obj is IResult result)
        {
            return result;
        }
        return Results.Ok(obj);
    }
}
