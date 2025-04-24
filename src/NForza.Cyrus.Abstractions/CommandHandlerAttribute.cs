using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public class CommandHandlerAttribute : Attribute
{
    public string Route { get; set; }
    public HttpVerb Verb { get; set; }
}
