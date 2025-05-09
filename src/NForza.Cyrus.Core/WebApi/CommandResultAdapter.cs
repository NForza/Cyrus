using System;
using System.Collections.Generic;
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

    public IResult FromObjectAndMessages((object Object, IEnumerable<object> Messages) commandResult)
    {
        eventBus.Publish(commandResult.Messages);
        return Results.Ok(commandResult.Object);
    }

    public IResult FromMessages(IEnumerable<object> Messages)
    {
        eventBus.Publish(Messages);
        return Results.Ok();
    }

    public IResult FromIResultAndMessages((IResult Result, IEnumerable<object> Messages) result)
    {
        eventBus.Publish(result.Messages);
        return result.Result;
    }

    public IResult FromIResultAndMessage((IResult Result, object Message) result)
    {
        eventBus.Publish([result.Message]);
        return result.Result;
    }
}
