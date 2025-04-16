using System;

namespace NForza.Cyrus.Cqrs;

public interface IQueryProcessor
{
    IServiceProvider ServiceProvider { get; }
}
