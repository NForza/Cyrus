using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public static class CommandResultBuilderExtensions
{
    public static CommandResultBuilder AcceptedOnEvent<T>(this CommandResultBuilder builder, string location = "")
        => builder.AddResultPolicy(new AcceptedOnEventPolicy<T>(location));

    public static CommandResultBuilder AcceptedWhenSucceeded(this CommandResultBuilder builder, string location = "")
        => builder.AddResultPolicy(new AcceptedWhenSucceededPolicy(location));

    public static CommandResultBuilder OtherwiseFail(this CommandResultBuilder builder, object? errorObject = null)
        => builder.AddResultPolicy(new OtherwiseFailPolicy(errorObject));

    public static CommandResultBuilder Result(this CommandResultBuilder builder, Func<CommandResult, IResult> resultFunc)
        => builder.AddResultPolicy(new ResultFuncPolicy(resultFunc));
}
