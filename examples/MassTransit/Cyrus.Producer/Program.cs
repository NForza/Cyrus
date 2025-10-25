using NForza.Cyrus.WebApi;
using MassTransit;
using System.Reflection;
using NForza.Cyrus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq();
});

var app = builder.Build();

app.MapCyrus();

app.Run();
