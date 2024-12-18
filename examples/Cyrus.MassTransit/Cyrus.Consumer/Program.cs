using System.Reflection;
using MassTransit;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.TypedIds;
using NForza.Cyrus.WebApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq((context, x) =>
    {
        x.ConfigureEndpoints(context);
    });
});

builder.Services.AddCyrus(options => options
    .AddMessagingServices()
    .AddTypedIdSerializers());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
