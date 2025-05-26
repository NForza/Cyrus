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

builder.Services.AddTransient<IAggregateRootPersistence<Track, TrackId>, EFAggregateRootPersistence<Track, TrackId, DemoContext>>();

builder.Services.AddCyrus();

var app = builder.Build();

app.MapCyrus();

app.MapPut("/tracks2/{TrackId:guid}", ([FromBody] global::TracksDemo.Tracks.Update.UpdateTrackCommandContract command, [FromServices] IEventBus eventBus, [FromServices] IHttpContextObjectFactory objectFactory, [FromServices] IHttpContextAccessor ctx) => {

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var (cmd, validationErrors) = objectFactory
        .CreateFromHttpContextWithBodyAndRouteParameters<global::TracksDemo.Tracks.Update.UpdateTrackCommandContract, global::TracksDemo.Tracks.Update.UpdateTrackCommand>(ctx.HttpContext, command);
    if (validationErrors.Any())
        return Results.BadRequest(validationErrors);

    var aggregatePersistence = services.GetRequiredService<IAggregateRootPersistence<Track, TrackId>>();

    var track = aggregatePersistence.Get(command.TrackId);
    var commandResult = services.GetRequiredService<global::TracksDemo.Tracks.Update.UpdateTrackCommandHandler>().Update2(cmd, track);
    aggregatePersistence.Save(track);
    return new CommandResultAdapter(eventBus).FromIResult(commandResult);
})
.WithOpenApi();


await app.RunAsync();