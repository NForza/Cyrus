using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi.Policies;

public class OtherwiseFailPolicy(object? errorObject) : CommandResultPolicy
{
    public override IResult? FromCommandResult(CommandResult result) => Results.Problem(JsonSerializer.Serialize(errorObject));
}