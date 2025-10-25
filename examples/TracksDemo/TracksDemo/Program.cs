using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NForza.Cyrus;
using NForza.Cyrus.WebApi;
using TracksDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DemoContext>(o =>
    o.UseInMemoryDatabase("DemoDb"));

builder.Services.AddCyrus(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();

public partial class Program { }