using System.IO;
using Microsoft.AspNetCore.Http;

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
        if (obj is Stream stream)
        {
            return Results.Stream(stream);
        }
        if (obj is (Stream, string))
        {
            (Stream file, string contentType) = ((Stream, string))obj;
            return Results.File(file, contentType);
        }
        return Results.Ok(obj);
    }
}
