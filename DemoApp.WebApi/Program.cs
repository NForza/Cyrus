using DemoApp.WebApi;
using NForza.Cqrs.WebApi;
using NForza.Cqrs;
using NForza.TypedIds;
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

//needs to be generated
builder.Services.AddEndpointGroup<CustomerEndpointGroup>();
builder.Services.AddTransient<IQueryFactory, HttpContextQueryFactory>();

builder.Services.AddTypedIds();
builder.Services.AddCqrs(o => o.AddCqrsEndpoints());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapCqrs();

await app.RunAsync();
