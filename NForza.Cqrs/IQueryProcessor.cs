﻿using System;
using System.Threading;

namespace NForza.Cqrs;

public interface IQueryProcessor
{
    TResult QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken);
    object QueryInternal(object query, Type queryType, CancellationToken cancellationToken);
}
