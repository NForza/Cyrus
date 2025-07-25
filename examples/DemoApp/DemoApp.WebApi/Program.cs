using System.Reflection;
using DemoApp.Domain.Customer;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;
using DemoApp.WebApi;
using Microsoft.EntityFrameworkCore;
using NForza.Cyrus.Abstractions;

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

app.UseCors("AllowAngularApp");

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapCyrus(logger);

await app.RunAsync();

//[GuidValue]
//public partial record struct SomeId;
