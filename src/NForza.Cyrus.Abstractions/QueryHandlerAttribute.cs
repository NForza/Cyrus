using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public class QueryHandlerAttribute: Attribute
{
    public string Route { get; set; } = null;
}
