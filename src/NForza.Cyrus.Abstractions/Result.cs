using System.IO;
using System.Net;

namespace NForza.Cyrus.Abstractions;

public class Result(HttpStatusCode statusCode)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public class SuccessResult<T>(HttpStatusCode statusCode, T value) : Result(statusCode)
{
    public T Value { get; } = value;
}

public class StreamResult(Stream stream) : SuccessResult<Stream>(HttpStatusCode.OK, stream) { }

public class OkResult(object value = null) : SuccessResult<object>(HttpStatusCode.OK, value) { }
public sealed class AcceptedResultArgs
{
    public string Location { get; set; }
    public object Body { get; set; }
}

public class AcceptedResult(AcceptedResultArgs value = null)
    : SuccessResult<AcceptedResultArgs>(HttpStatusCode.Accepted, value ?? new())
{ }
public class FileResult(FileResultArgs fileResultArgs) : SuccessResult<FileResultArgs>(HttpStatusCode.OK, fileResultArgs) { }

public class ErrorResult(HttpStatusCode statusCode, Error error = null) : Result(statusCode)
{
    public Error Error { get; } = error;
}

public class BadRequestResult(Error error) : ErrorResult(HttpStatusCode.BadRequest, error) { }

public class NotFoundResult() : ErrorResult(HttpStatusCode.NotFound) { }

public record struct FileResultArgs(Stream Stream, string ContentType) { }