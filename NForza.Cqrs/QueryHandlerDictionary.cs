using System;
using System.Collections.Generic;
using System.Threading;

namespace NForza.Cqrs;

public class QueryHandlerDictionary : Dictionary<Type, Func<object, object>>
{
    internal Func<TQuery, CancellationToken, TResult> GetHandler<TQuery, TResult>()
    {
        return (query, cancellationToken) => default;
    }
}