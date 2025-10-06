using Microsoft.AspNetCore.Http;
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

        if (result.IsFailure)
            return MapError(result.Error);

        var type = result.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueProp = type.GetProperty("Value");    
            var value = valueProp!.GetValue(result);

            return value switch
            {
                null => Results.NoContent(),
                FileResult fileResult => Results.File(fileResult.Stream, fileResult.ContentType),
                StreamResult fileResult => Results.Stream(fileResult.Stream),
                AcceptedResult acceptedResult => Results.Accepted(acceptedResult.Location, acceptedResult.Value),
                _ => Results.Ok(value)
            };
        }
        return Results.NoContent();
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
