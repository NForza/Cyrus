using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.WebApi;

public static  class ResultExtensions
{
    public static IResult ToIResult(this Result result)
        => result.IsSuccess ? Results.Ok() : Results.BadRequest();
}
