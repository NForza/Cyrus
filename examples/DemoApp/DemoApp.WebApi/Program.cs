using System.Reflection;
using DemoApp.Domain.Customer;
using FluentValidation;
using MassTransit;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;
using NForza.Cyrus.SignalR;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Abstractions.Model;
using Microsoft.Extensions.DependencyModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly(), typeof(Customer).Assembly);
    cfg.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<Customer>();

builder.Services.AddCyrus(o => o.AddEndpointGroups().AddTypedIdSerializers().AddSignalRHubs());

builder.Services.Scan(scan => scan
    .FromDependencyContext(DependencyContext.Default!)
    .AddClasses(classes => classes.AssignableTo<ICyrusModel>())
    .AsImplementedInterfaces()
    .WithTransientLifetime());

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.MapCyrus();

ICyrusModel model = CyrusModel.Aggregate(app.Services);
Console.WriteLine(model.AsJson());

await app.RunAsync();
