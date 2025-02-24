using System.Reflection;
using DemoApp.Domain.Customer;
using FluentValidation;
using MassTransit;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.SignalR;

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

builder.Services.AddCyrus(o => o.AddTypedIdSerializers().AddSignalRHubs());

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.MapCyrus();

await app.RunAsync();
