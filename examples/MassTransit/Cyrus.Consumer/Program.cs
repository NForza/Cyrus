using System.Reflection;
using MassTransit;
using NForza.Cyrus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCyrus(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq((context, x) =>
    {
        x.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.Run();