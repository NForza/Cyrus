using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public class CommandResultAdapter(IMessageBus eventBus)
{
    public IResult FromObjects(object obj)
    {
        return new OkResult(obj).ToIResult();
    }

    public IResult FromVoid()
    {
        return new OkResult().ToIResult();
    }

    public IResult FromResult(Result result)
    {
        return result.ToIResult();
    }

    public IResult FromObjectAndMessages((object Object, IEnumerable<object> Messages) commandResult)
    {
        eventBus.Publish(commandResult.Messages);
        return new OkResult(commandResult.Object).ToIResult();
    }

    public IResult FromMessages(IEnumerable<object> Messages)
    {
        eventBus.Publish(Messages);
        return new OkResult().ToIResult();
    }

    public IResult FromResultAndMessages((Result Result, IEnumerable<object> Messages) result)
    {
        eventBus.Publish(result.Messages);
        return result.Result.ToIResult();
    }

    public IResult FromResultAndMessage((Result Result, object Message) result)
    {
        eventBus.Publish([result.Message]);
        return result.Result.ToIResult();
    }
}
