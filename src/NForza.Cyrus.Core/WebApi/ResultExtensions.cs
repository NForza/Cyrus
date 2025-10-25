using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.WebApi;

using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

public static class ResultExtensions
{
    public static IResult ToIResult(this Result result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));

        if (result is ErrorResult errorResult)
            return MapError(errorResult.Error);

        return result switch
        {
            null => Results.NoContent(),
            FileResult fileResult => Results.File(fileResult.Value.Stream, fileResult.Value.ContentType),
            StreamResult streamResult => Results.Stream(streamResult.Value),
            AcceptedResult acceptedResult => Results.Accepted(acceptedResult.Value.Location, acceptedResult.Value.Body),
            OkResult okResult => Results.Ok(okResult.Value),
            BadRequestResult badRequestResult => Results.BadRequest(badRequestResult.Error),
            Result someResult => Results.StatusCode((int)someResult.StatusCode),
        };
    }

    private static IResult MapError(Error error)
    {
        return error.ErrorType switch
        {
            ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
            ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),
            ErrorType.Invalid => Results.BadRequest(error.InnerErrors.Select(e => new { e.Code, e.Message })),
            _ => Results.Problem(JsonSerializer.Serialize(error.InnerErrors.Select(e => new { e.Code, e.Message })))
        };
    }
}
