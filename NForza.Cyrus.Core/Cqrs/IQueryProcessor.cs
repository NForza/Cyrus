using System;
using System.Threading;

namespace NForza.Cyrus.Cqrs;

public interface IQueryProcessor
{
    TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken);
    object QueryInternal(object query, Type queryType, CancellationToken cancellationToken);
}
