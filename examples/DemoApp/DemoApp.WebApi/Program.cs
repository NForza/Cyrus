using System.Reflection;
using System.Linq;
using DemoApp.Domain.Customer;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;
using DemoApp.WebApi;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly(), typeof(Customer).Assembly);
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddDbContext<DemoDbContext>(o => o.UseInMemoryDatabase("Demo.Webapi"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddCyrus();

var app = builder.Build();

app.MapDelete("c2/{Id:guid}", async (Guid Id, [FromBody] global::DemoApp.Contracts.Customers.DeleteCustomerCommandContract command, [FromServices] IEventBus eventBus, [FromServices] IHttpContextObjectFactory objectFactory, [FromServices] IHttpContextAccessor ctx) => {

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var (cmd, validationErrors) = objectFactory
        .CreateFromHttpContextWithBodyAndRouteParameters<global::DemoApp.Contracts.Customers.DeleteCustomerCommandContract, global::DemoApp.Contracts.Customers.DeleteCustomerCommand>(ctx.HttpContext, command);
    if (validationErrors.Any())
        return Results.BadRequest(validationErrors);

    ICommandDispatcher dispatcher = services.GetRequiredService<ICommandDispatcher>();
    var commandResult = await dispatcher.Handle(cmd);
    return new CommandResultAdapter(eventBus).FromIResult(commandResult);
})
.WithOpenApi();

app.UseCors("AllowAngularApp");

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapCyrus(logger);

await app.RunAsync();