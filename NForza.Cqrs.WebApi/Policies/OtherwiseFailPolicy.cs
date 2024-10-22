using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace NForza.Cqrs.WebApi.Policies;

public class OtherwiseFailPolicy(object? errorObject) : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result) => Results.Problem(JsonSerializer.Serialize(errorObject));
}