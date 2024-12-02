using NForza.Cyrus.Cqrs;
using NForza.Cyrus.TypedIds;
using NForza.Cyrus.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTypedIds();
builder.Services.AddCqrs(options => options.AddEndpointGroups());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCqrs();
app.Run();