using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NForza.Cqrs;

public class HandlerDictionary : Dictionary<Type, Func<IServiceProvider, object, Task<CommandResult>>>
{
    public void AddHandler<T>(Func<IServiceProvider, object, Task<CommandResult>> handler)
    {
        Add(typeof(T), (services, c) => handler(services, c));
    }
}