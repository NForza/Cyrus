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

    public IResult FromIResult(IResult result)
    {
        return result;
    }

    public IResult FromObjectAndEvents((object Object, IEnumerable<Object> Events) commandResult)
    {
        eventBus.Publish(commandResult.Events);
        return Results.Ok(commandResult.Object);
    }

    public IResult FromEvents(IEnumerable<Object> events)
    {
        eventBus.Publish(events);
        return Results.Ok();
    }

    public IResult FromIResultAndEvents((IResult Result, IEnumerable<Object> Events) result)
    {
        eventBus.Publish(result.Events);
        return result.Result;
    }
}
