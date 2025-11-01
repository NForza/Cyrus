using System.Reflection;
using MassTransit;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCyrus(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq();
});

var app = builder.Build();

app.MapCyrus();

app.Run();
