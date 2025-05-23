﻿using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class EventAttribute : Attribute
{
    public bool Local { get; set; } = false;
}
