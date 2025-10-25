using System.IO;

namespace NForza.Cyrus.Abstractions;

public static class ResultExtensions
{
    extension(Result)
    {
        public static Result Ok(object value) => new OkResult(value);
        public static Result Accepted(string location = "", object body = null) => new AcceptedResult(new AcceptedResultArgs { Location = location, Body = body });
        public static Result BadRequest(string message = "") => new BadRequestResult(message);
        public static Result NotFound() => new NotFoundResult();
        public static Result File(Stream stream, string contentType) => new FileResult(new FileResultArgs(stream, contentType));
        public static Result Stream(Stream stream) => new StreamResult(stream);
        public static Result Conflict(string message = "") => new ConflictResult(message);
    }
}
