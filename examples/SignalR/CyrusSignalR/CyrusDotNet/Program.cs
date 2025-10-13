using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;

// This is the Cyrus SignalR server application.
// The Angular client app is located in ../CyrusSignalR-Angular
// npm install && ng serve to start it.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapCyrus();

app.UseCors("AllowAngularClient");

app.Run();
