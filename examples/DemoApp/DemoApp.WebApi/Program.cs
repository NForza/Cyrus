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
using Microsoft.AspNetCore.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddMassTransit(cfg =>
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
    IMessageBus bus = app.Services.GetRequiredService<IMessageBus>();
    bus.Publish(new AddCustomerCommand(new CustomerId(), new Name("Thomas"), new Address(new Street("TestStreet"), new StreetNumber(1)), CustomerType.Private));
    //var rc = bus.CreateRequestClient<AddCustomerCommand>();
    //Response<AcceptedResult> resultResponse = await rc.GetResponse<AcceptedResult>(new AddCustomerCommand(new CustomerId(), new Name("Thomas"), new Address(new Street("TestStreet"), new StreetNumber(1)), CustomerType.Private));
    //var result = resultResponse.Message;
    //Console.WriteLine(JsonSerializer.Serialize(result));
});

app.MapCyrus(logger).MapAsyncApi();

await app.RunAsync();
