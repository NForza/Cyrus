using NForza.Lumia.TypedIds;
using NForza.Lumia.Cqrs;
using NForza.Lumia.Cqrs.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTypedIds();
builder.Services.AddCqrs(o => o.AddEndpointGroups());

var app = builder.Build();
app.MapCqrs();

await app.RunAsync();
