using System.Reflection;
using DemoApp.Contracts.Customers;
using DemoApp.Domain.Customer;
using FluentValidation;
using MassTransit;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly(), typeof(Customer).Assembly);
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

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

builder.Services.AddValidatorsFromAssemblyContaining<Customer>();
builder.Services.AddCyrus();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");

ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapCyrus(logger);

await app.RunAsync();