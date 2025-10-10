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
using NForza.Cyrus.Cqrs;
using DemoApp.Contracts.Customers;
using DemoApp.Contracts;
using NForza.Cyrus.Abstractions;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
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

Task.Run(async () =>
{
    await Task.Delay(5000); // Wait for the bus to be ready
    IBus bus = app.Services.GetRequiredService<IBus>();
    var rc = bus.CreateRequestClient<AddCustomerCommand>();
    Response<Result> resultResponse = await rc.GetResponse<Result>(new AddCustomerCommand(new CustomerId(), new Name("Thomas"), new Address(new Street("TestStreet"), new StreetNumber(1)), CustomerType.Private));
    var result = resultResponse.Message;
    Console.WriteLine(result.IsSuccess);
});

app.MapCyrus(logger).MapAsyncApi();

await app.RunAsync();
