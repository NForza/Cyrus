using System.IO;
using Microsoft.AspNetCore.Http;

namespace NForza.Cyrus.WebApi;

public static class QueryResultAdapter
{
    public static Result FromObject<T>(object obj)
    {
        if (obj == null)
        {
            return Result.Failure(ErrorFactory<Result>.NotFound<T>());
        }
        if (obj is Result result)
        {
            return result;
        }
        if (obj is Stream stream)
        {
            return Result.Stream(stream);
        }
        if (obj is (Stream, string))
        {
            (Stream file, string contentType) = ((Stream, string))obj;
            return Result.File(file, contentType);
        }
        return Result.Success(obj);
    }
}
