using System.Reflection;
using TracksDemo.Tracks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NForza.Cyrus;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;
using TracksDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DemoContext>(o => 
    o.UseInMemoryDatabase("DemoDb"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    x.UsingInMemory((context, cfg) =>cfg.ConfigureEndpoints(context));
});

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

await app.RunAsync();