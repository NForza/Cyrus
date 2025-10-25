using MassTransit;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;
using SimpleCyrusWebApi.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddFilter("Default", LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

//Add in-memory EF Core database
builder.Services.AddDbContext<DemoContext>();

builder.Services.AddCyrus(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();