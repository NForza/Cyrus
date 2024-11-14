using DemoApp.WebApi;
using Microsoft.Extensions.Options;
using NForza.Cqrs.WebApi;
using NForza.Cqrs;
using NForza.TypedIds;
using Swashbuckle.AspNetCore.SwaggerGen;
using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointGroup<CustomerEndpointGroup>();
builder.Services.AddCqrs(o => o.AddCqrsEndpoints());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTypedIds();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapCqrs();
app.UseHttpsRedirection();
await app.RunAsync();
