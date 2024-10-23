using DemoApp.WebApi;
using Microsoft.Extensions.Options;
using NForza.Cqrs;
using NForza.TypedIds;
using NForza.Cqrs.WebApi;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

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
