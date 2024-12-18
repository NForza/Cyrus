using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Cqrs;

public static class AssemblyExtensions
{
    public static bool IsFrameworkAssembly(this IAssemblySymbol assembly)
    {
        var name = assembly?.Name;
        return name == null
               || name.StartsWith("System")
               || name.StartsWith("Microsoft")
               || name.StartsWith("MassTransit")
               || name == "mscorlib"
               || name == "netstandard"
               || name == "WindowsBase";
    }
}
