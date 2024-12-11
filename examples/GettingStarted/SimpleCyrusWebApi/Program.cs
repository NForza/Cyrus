using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.MassTransit;
using NForza.Cyrus.TypedIds;
using NForza.Cyrus.WebApi;
using SimpleCyrusWebApi.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DemoContext>();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

//Adds all Cyrus services
builder.Services.AddCyrus(options => options
        .AddEndpointGroups()
        .AddTypedIdSerializers());

builder.Services.Replace(ServiceDescriptor.Singleton<IEventBus, MassTransitEventBus>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Exposes all endpoints in all EndpointGroups
app.MapCqrs();
app.Run();