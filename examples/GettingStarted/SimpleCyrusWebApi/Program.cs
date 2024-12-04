using Microsoft.EntityFrameworkCore;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.TypedIds;
using NForza.Cyrus.WebApi;
using SimpleCyrusWebApi.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DemoContext>();

//Adds the JsonConverters for all the TypedIds
builder.Services.AddTypedIds();

//Adds all Cyrus Cqrs  services
builder.Services.AddCqrs(
    //Configures all endpoints in all EndpointGroups
    options => options.AddEndpointGroups());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Exposes all endpoints in all EndpointGroups
app.MapCqrs();
app.Run();