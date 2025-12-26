using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DemoApp.WebApi;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

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

builder.Services.AddCyrus(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly(), typeof(DemoApp.Domain.CyrusConfiguration).Assembly);
    cfg.SetSnakeCaseEndpointNameFormatter();
    cfg.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
    //cfg.UsingInMemory((context, cfg) =>
    //{
    //    cfg.ConfigureEndpoints(context);
    //});
});

var app = builder.Build();

app.MapPost("customers2", async ([FromBody] global::DemoApp.Contracts.Customers.AddCustomerCommandContract command) =>
{
    return CommandDispatch.ExecuteAsync<global::DemoApp.Contracts.Customers.AddCustomerCommandContract, global::DemoApp.Contracts.Customers.AddCustomerCommand>(
        app, 
        command,
        ctx => ctx.Services.GetRequiredService<global::DemoApp.Domain.Customer.AddCustomerCommandValidator>().Validate((global::DemoApp.Contracts.Customers.AddCustomerCommand) ctx.Command!),
        async (dispatcher, messageBus, cmd) =>
        {
            var commandResult = await dispatcher.Handle(cmd);
            return new CommandResultAdapter(messageBus).FromResultAndMessages(commandResult);
        });
})
.WithOpenApi()
.WithMetadata([new global::Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(202)]);

app.UseCors("AllowAngularApp");

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapCyrus(logger).MapAsyncApi();

await app.RunAsync();