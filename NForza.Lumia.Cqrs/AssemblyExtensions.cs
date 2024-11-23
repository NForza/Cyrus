﻿using System.Reflection;

namespace NForza.Lumia.Cqrs;

public static class AssemblyExtensions
{
    public static bool IsFrameworkAssembly(this Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return name == null || name.StartsWith("System") || name.StartsWith("Microsoft");
    }
}