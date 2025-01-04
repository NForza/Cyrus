using NForza.Cyrus.TypedIds;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;
using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingRabbitMq();
});

builder.Services.AddCyrus(o => o
    .AddEndpointGroups()
    .AddTypedIdSerializers());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapCqrs();

app.Run();
