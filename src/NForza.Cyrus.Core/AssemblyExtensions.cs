using System.Linq;
using System.Reflection;

namespace NForza.Cyrus;

public static class AssemblyExtensions
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

    private static string[] frameworkOrCyrusAssemblies = [ ..frameworkAssembliesToSkip, "NForza.Cyrus" ];

    public static bool IsFrameworkAssembly(this Assembly assembly)
    {
        string? assemblyName = assembly.GetName().Name;
        return assemblyName == null || frameworkAssembliesToSkip.Any(n => assemblyName.StartsWith(n));
    }

    public static bool IsFrameworkOrCyrusAssembly(this Assembly assembly)
    {
        string? assemblyName = assembly.GetName().Name;
        return assemblyName == null || frameworkOrCyrusAssemblies.Any(n => assemblyName.StartsWith(n));
    }

}
