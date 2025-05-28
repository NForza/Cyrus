using System;

namespace NForza.Cyrus.Cqrs;

public interface ICommandDispatcher
{
    IServiceProvider Services { get; }
}