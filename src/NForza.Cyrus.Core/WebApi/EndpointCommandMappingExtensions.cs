#nullable enable
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public static partial class EndpointCommandMappingExtensions
{
    public static IEndpointRouteBuilder MapCommands(this IEndpointRouteBuilder endpoints)
    {
        var commandEndpoints = endpoints.ServiceProvider.GetServices<CommandEndpointDefinition>();

        foreach (var commandEndpoint in commandEndpoints)
        {
            MapCommand(endpoints, commandEndpoint);
        }
        return endpoints;
    }

    internal static RouteHandlerBuilder MapCommand(IEndpointRouteBuilder endpoints, CommandEndpointDefinition endpointDefinition)
        => endpoints.MapMethods(endpointDefinition.Path, [endpointDefinition.Method], async (HttpContext ctx, [FromServices] ICommandDispatcher commandDispatcher) =>
                {
                    var commandObject = await CreateCommandObject(endpointDefinition, ctx);

                    if (commandObject == null)
                        return Results.BadRequest("Invalid command object.");

                    if (!ValidateObject(endpointDefinition.EndpointType, commandObject, ctx.RequestServices, out var problem))
                        return Results.BadRequest(problem);

                    var commandResult = await commandDispatcher.ExecuteInternalAsync(commandObject, CancellationToken.None);

                    var commandResultAttributes = endpointDefinition.CommandResultPolicies;
                    foreach (var policy in commandResultAttributes)
                    {
                        var result = policy.FromCommandResult(commandResult);
                        if (result != null)
                            return result;
                    }

                    return commandResult.Succeeded ? Results.Ok(commandResult) : Results.Problem(JsonSerializer.Serialize(commandResult.Errors));
                })
            .WithOpenApi(operation =>
                {
                    foreach (var param in FindAllParametersInRoute(endpointDefinition.Path))
                    {
                        operation.Parameters.Add(
                            new OpenApiParameter()
                            {
                                Name = param,
                                In = ParameterLocation.Path,
                                Required = true,
                                Schema = new OpenApiSchema() { Type = "string" }
                            });
                    }
                    return operation;
                })
            .Accepts(endpointDefinition.EndpointType, MediaTypeNames.Application.Json)
            .WithTags(endpointDefinition.Tags);


    static IEnumerable<string> FindAllParametersInRoute(string route)
    {
        var rgx = ParameterRegex();
        MatchCollection matches = rgx.Matches(route);
        return matches.Select(m => m.Groups["parameter"].Value);
    }


    internal static bool ValidateObject(Type objectType, object queryObject, IServiceProvider serviceProvider, out object? problem)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(objectType);
        if (serviceProvider.GetService(validatorType) is not IValidator validator)
        {
            problem = null;
            return true;
        }

        var validationContextType = typeof(ValidationContext<>).MakeGenericType(objectType);
        var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, queryObject)!;

        ValidationResult validationResult = validator?.Validate(validationContext) ?? throw new InvalidOperationException($"Can't validate {objectType.FullName}");
        if (validationResult.IsValid)
        {
            problem = null;
            return true;
        }
        problem = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        return false;
    }

    private static async Task<object?> CreateCommandObject(CommandEndpointDefinition endpointDefinition, HttpContext ctx)
    {
        var inputMappingPolicy = endpointDefinition.InputMappingPolicyType != null ?
            (InputMappingPolicy)ctx.RequestServices.GetRequiredService(endpointDefinition.InputMappingPolicyType) : new DefaultCommandInputMappingPolicy(ctx);
        return await inputMappingPolicy.MapInputAsync(endpointDefinition.EndpointType);
    }

    [GeneratedRegex(@"\{(?<parameter>\w+)\}")]
    private static partial Regex ParameterRegex();
}
