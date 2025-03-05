using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

public static class AssemblyExtensions
{
    public static bool IsFrameworkAssembly(this IAssemblySymbol assembly)
    {
        var name = assembly?.Name;
        return name == null
               || name.StartsWith("System")
               || name.StartsWith("Microsoft")
               || name.StartsWith("MassTransit")
               || name.StartsWith("RabbitMQ")
               || name.StartsWith("NForza.Cyrus")
               || name == "mscorlib"
               || name == "netstandard"
               || name == "WindowsBase";
    }

    public static bool ReferencesCyrusAbstractions(this IAssemblySymbol assembly)
    {
        var visited = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
        return ReferencesAssemblyRecursive(assembly, "NForza.Cyrus.Abstractions", visited);
    }

    public static bool ReferencesAssembly(this IAssemblySymbol assembly, string targetAssemblyName)
    {
        var visited = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
        return ReferencesAssemblyRecursive(assembly, targetAssemblyName, visited);
    }

    private static bool ReferencesAssemblyRecursive(this IAssemblySymbol assembly, string targetAssemblyName, HashSet<IAssemblySymbol> visited)
    {
        if (assembly.IsFrameworkAssembly())
        {
            return false;
        }
        if (assembly.Name == targetAssemblyName)
        {
            return true;
        }
        if (!visited.Add(assembly))
        {
            return false;
        }

        return assembly.Modules
            .SelectMany(m => m.ReferencedAssemblies)
            .Any(reference => reference.Name == targetAssemblyName) ||
                assembly.Modules
                    .SelectMany(m => m.ReferencedAssemblySymbols)
                    .Any(reference => ReferencesAssemblyRecursive(reference, targetAssemblyName, visited));
    }
}
