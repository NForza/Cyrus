using System.Reflection;
using DemoApp.WebApi;
using MassTransit;
using Microsoft.AspNetCore.Builder;
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

app.UseCors("AllowAngularApp");

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapCyrus(logger).MapAsyncApi();

await app.RunAsync();
