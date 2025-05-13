using System.Reflection;
using CyrusDemo;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DemoContext>(options =>
    options.UseInMemoryDatabase("DemoDb"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    x.UsingInMemory((context, cfg) =>cfg.ConfigureEndpoints(context));
});

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();