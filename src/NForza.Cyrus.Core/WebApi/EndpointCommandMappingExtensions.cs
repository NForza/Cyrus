#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public static partial class EndpointCommandMappingExtensions
{
    public static IEndpointRouteBuilder MapCommands(this IEndpointRouteBuilder endpoints)
    {
        var commands = endpoints.ServiceProvider.GetRequiredService<CommandHandlerDictionary>();

        foreach (var command in commands)
        {
            MapCommand(endpoints, command.Key, command.Value);
        }
        return endpoints;
    }

    internal static RouteHandlerBuilder MapCommand(IEndpointRouteBuilder endpoints, Type commandType, CommandHandlerDefinition commandHandlerDefinition)
        => endpoints.MapMethods(commandHandlerDefinition.Route, [commandHandlerDefinition.Verb.ToString()], async (HttpContext ctx, [FromServices] ICommandDispatcher commandDispatcher) =>
                {
                    object? commandObject = null;
                    try
                    {
                        commandObject = ctx.RequestServices.GetRequiredService<DefaultCommandInputMappingPolicy>().MapInputAsync(commandType);
                    }
                    catch(JsonException jsonException)
                    {
                        return Results.BadRequest(jsonException.Message);
                    }
                    catch(Exception ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
           
                    if (!ValidateObject(commandType, commandObject, ctx.RequestServices, out var problem))
                        return Results.BadRequest(problem);

                    var commandResult = await commandDispatcher.ExecuteInternalAsync(commandObject, CancellationToken.None);

                    return commandResult.Succeeded ? Results.Ok(commandResult) : Results.Problem(JsonSerializer.Serialize(commandResult.Errors));
                })
            .WithSwaggerParameters(commandHandlerDefinition.Route)
            .Accepts(commandType, MediaTypeNames.Application.Json);

    internal static bool ValidateObject(Type objectType, object? queryObject, IServiceProvider serviceProvider, out object? problem)
    {
        if (objectType == null)
        {
            problem = "Can't create Query object of Type " + objectType?.Name;
            return false;
        }
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
}
