﻿using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class LocalCommandBus(IServiceScopeFactory serviceScopeFactory, CommandHandlerDictionary handlers) : ICommandBus
{
    public Task<CommandResult> Execute(object command, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = handlers[command.GetType()]?.Handler ?? throw new InvalidOperationException($"{command.GetType().Name} has no registered handler");
        return handler(scope.ServiceProvider, command);
    }
}
