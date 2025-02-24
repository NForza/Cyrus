using System.Reflection;
using FluentValidation;
using MassTransit;
using NForza.Cyrus.Cqrs;
using SimpleCyrusWebApi.Model;
using SimpleCyrusWebApi.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddFilter("Default", LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add in-memory EF Core database
builder.Services.AddDbContext<DemoContext>();

//Add in-memory MassTransit bus
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly(), typeof(Customer).Assembly]);

builder.Services.AddCyrus();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Exposes all endpoints in all EndpointGroups
app.MapCyrus();
app.Run();