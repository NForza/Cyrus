using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public class CommandResultAdapter(IEventBus eventBus)
{
    public IResult FromObjects(object obj)
    {
        return Results.Ok(obj);
    }

    public IResult FromVoid()
    {
        return Results.Ok();
    }

    public IResult FromObjectAndEvents((object Object, IEnumerable<Object> Events) commandResult)
    {
        commandResult.Events.ToList().ForEach(@event => eventBus.Publish(@event));
        return Results.Ok(commandResult.Object);
    }

    public IResult FromEvents(IEnumerable<Object> events)
    {
        events.ToList().ForEach(@event => eventBus.Publish(@event));
        return Results.Ok();
    }

    public IResult FromIResultAndEvents((IResult Result, IEnumerable<Object> Events) result)
    {
        result.Events.ToList().ForEach(@event => eventBus.Publish(@event));
        return result.Result;
    }
}
