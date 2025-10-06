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
            return Result.Failure(ErrorFactory<Result>.NotFound<T>()).ToIResult();
        }
        if (obj is Result result)
        {
            return result.ToIResult();
        }
        if (obj is Stream stream)
        {
            return Result.Stream(stream).ToIResult();
        }
        if (obj is (Stream, string))
        {
            (Stream file, string contentType) = ((Stream, string))obj;
            return Result.File(file, contentType).ToIResult();
        }
        return Result.Success(obj).ToIResult();
    }
}
