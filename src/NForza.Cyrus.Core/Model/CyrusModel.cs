using System.Linq;
using System.Reflection;
using MassTransit;

namespace NForza.Cyrus.Model;

public static class CyrusModel
{
    public static string AsJson()
    {
        Assembly assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            return string.Empty;
        }
        var attribute = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault( a => a.Key == "cyrus-model");
        return attribute?.Value?.DecompressFromBase64() ?? string.Empty;
    }

    public static string AsAsyncApiYaml(IBus bus)
    {
        return string.Empty;
    }
}