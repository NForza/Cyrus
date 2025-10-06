using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class CommandDispatcher(IServiceScopeFactory serviceScopeFactory) : ICommandDispatcher
{
    public IServiceProvider Services => serviceScopeFactory.CreateScope().ServiceProvider;
}