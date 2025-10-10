using System.IO;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.WebApi;

public static class QueryResultAdapter
{
    public static IResult FromObject<T>(object obj)
    {
        if (obj == null)
        {
            return new NotFoundResult().ToIResult();
        }
        if (obj is Result result)
        {
            return result.ToIResult();
        }
        if (obj is Stream stream)
        {
            return new StreamResult(stream).ToIResult();
        }
        if (obj is (Stream, string))
        {
            (Stream file, string contentType) = ((Stream, string))obj;
            return new FileResult(new() { Stream = file, ContentType = contentType }).ToIResult();
        }
        return new OkResult(obj).ToIResult();
    }
}
