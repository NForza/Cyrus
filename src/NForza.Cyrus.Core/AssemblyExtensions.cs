using System.Reflection;

namespace NForza.Cyrus;

public static class AssemblyExtensions
{
    private static string[] assembliesToSkip = ["System", "Microsoft", "mscorlib", "netstandard", "WindowsBase", "Swashbuckle", "RabbitMQ", "MassTransit"];

    public static bool IsFrameworkAssembly(this Assembly assembly)
    {
        string? assemblyName = assembly.GetName().Name;
        return assemblyName == null || assembliesToSkip.Any(n => assemblyName.StartsWith(n));
    }
}
