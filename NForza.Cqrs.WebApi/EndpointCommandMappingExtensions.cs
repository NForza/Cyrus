#nullable enable
using System.Net.Mime;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs.WebApi;

public static class EndpointCommandMappingExtensions
{
    public static IEndpointRouteBuilder MapCommands(this IEndpointRouteBuilder endpoints)
    {
        var commandEndpoints = endpoints.ServiceProvider.GetServices<CommandEndpointDefinition>();

        foreach (var commandEndpoint in commandEndpoints)
        {
            var methodAttribute = commandEndpoint.Method;
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

                    var commandResult = await commandDispatcher.ExecuteInternal(commandObject, CancellationToken.None);

                    var commandResultAttributes = endpointDefinition.CommandResultPolicies;
                    foreach (var policy in commandResultAttributes)
                    {
                        var result = policy.FromCommandResult(commandResult);
                        if (result != null)
                            return result;
                    }

                    return commandResult.Succeeded ? Results.Ok(commandResult) : Results.Problem(JsonSerializer.Serialize(commandResult.Errors));
                })
            .WithOpenApi()
            .Accepts(endpointDefinition.EndpointType, MediaTypeNames.Application.Json)
            .WithTags(endpointDefinition.Tags)
        ;

    internal static bool ValidateObject(Type objectType, object queryObject, IServiceProvider serviceProvider, out object? problem)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(objectType);
        IValidator? validator = serviceProvider.GetService(validatorType) as IValidator;
        if (validator == null)
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
}
