using DemoApp.WebApi;
using NForza.Cqrs;
using NForza.Cqrs.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<EndpointGroup, CustomerEndpointGroup>();
builder.Services.AddCqrs(o => o.AddEndpoints());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapCqrs();
app.UseHttpsRedirection();
app.Run();

