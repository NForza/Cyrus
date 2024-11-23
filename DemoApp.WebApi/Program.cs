using MassTransit;
using NForza.Lumia.TypedIds;
using NForza.Lumia.Cqrs;
using NForza.Lumia.Cqrs.WebApi;
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

builder.Services.AddTypedIds();
builder.Services.AddCqrs(o => o.AddEndpointGroups());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

}
app.MapCqrs();

await app.RunAsync();
