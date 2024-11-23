using Microsoft.AspNetCore.Http;

namespace NForza.Lumia.Cqrs.WebApi.Policies;

public static class CommandEndpointBuilderExtensions
{
    public static CommandEndpointBuilder AcceptedOnEvent<T>(this CommandEndpointBuilder builder, string location = "")
        => builder.AddResultPolicy(new AcceptedOnEventPolicy<T>(location));

    public static CommandEndpointBuilder OtherwiseFail(this CommandEndpointBuilder builder, object? errorObject = null)
        => builder.AddResultPolicy(new OtherwiseFailPolicy(errorObject));

    public static CommandEndpointBuilder Result(this CommandEndpointBuilder builder, Func<CommandResult, IResult> resultFunc)
        => builder.AddResultPolicy(new ResultFuncPolicy(resultFunc));


}
