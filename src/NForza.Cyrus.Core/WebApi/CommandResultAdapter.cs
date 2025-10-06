using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public class CommandResultAdapter(IMessageBus eventBus)
{
    public Result FromObjects(object obj)
    {
        return Result.Success(obj);
    }

    public Result FromVoid()
    {
        return Result.Success();
    }

    public Result FromResult(Result result)
    {
        return result;
    }

    public Result FromObjectAndMessages((object Object, IEnumerable<object> Messages) commandResult)
    {
        eventBus.Publish(commandResult.Messages);
        return Result.Success(commandResult.Object);
    }

    public Result FromMessages(IEnumerable<object> Messages)
    {
        eventBus.Publish(Messages);
        return Result.Success();
    }

    public Result FromResultAndMessages((Result Result, IEnumerable<object> Messages) result)
    {
        eventBus.Publish(result.Messages);
        return result.Result;
    }

    public Result FromResultAndMessage((Result Result, object Message) result)
    {
        eventBus.Publish([result.Message]);
        return result.Result;
    }
}
