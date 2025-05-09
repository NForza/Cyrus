﻿namespace NForza.Cyrus.Generators.SignalR;

public class SignalREvent
{
    public string Name { get; internal set; } = string.Empty;
    public string FullTypeName { get; internal set; } = string.Empty;
    public string MethodName { get; internal set; } = string.Empty;
    public bool Broadcast { get; set; }
}
