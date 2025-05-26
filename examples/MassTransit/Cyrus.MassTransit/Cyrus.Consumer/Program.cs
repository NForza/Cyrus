using System.Reflection;
using MassTransit;
using NForza.Cyrus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq((context, x) =>
    {
        x.ConfigureEndpoints(context);
    });
});

builder.Services.AddCyrus();

var app = builder.Build();

app.Run();