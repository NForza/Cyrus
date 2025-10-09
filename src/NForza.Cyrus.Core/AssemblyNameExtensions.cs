using System.Linq;
using System.Reflection;

namespace NForza.Cyrus;

public static class AssemblyNameExtensions
{
    private static string[] frameworkAssembliesToSkip = [
        "System",
        "Microsoft",
        "mscorlib",
        "netstandard",
        "WindowsBase",
        "Scalar",
        "RabbitMQ",
        "MassTransit",
    ];

    private static string[] frameworkOrCyrusAssemblies = [.. frameworkAssembliesToSkip, "NForza.Cyrus"];

    public static bool IsFrameworkAssembly(this AssemblyName assemblyName)
    {
        string? fullAssemblyName = assemblyName.Name;
        return fullAssemblyName == null || frameworkAssembliesToSkip.Any(n => fullAssemblyName.StartsWith(n));
    }

    public static bool IsFrameworkOrCyrusAssembly(this AssemblyName assemblyName)
    {
        string? fullAssemblyName = assemblyName.Name;
        return fullAssemblyName == null || frameworkOrCyrusAssemblies.Any(n => fullAssemblyName.StartsWith(n));
    }
}