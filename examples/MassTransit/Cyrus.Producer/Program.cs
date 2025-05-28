using NForza.Cyrus.WebApi;
using MassTransit;
using System.Reflection;
using NForza.Cyrus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq();
});

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

app.Run();
