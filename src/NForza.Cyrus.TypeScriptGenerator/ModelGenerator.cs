
using System.Reflection;
using NForza.Cyrus.Model;

namespace NForza.Cyrus.TypeScriptGenerator;

internal class ModelGenerator(string modelAssemblyFile)
{
    internal (bool succeeded, string? model) GetModel()
    {
        if (!File.Exists(modelAssemblyFile))
        {
            Console.WriteLine($"Model assembly file not found: {modelAssemblyFile}");
            return (false, null);
        }

        var loadContext = new PluginLoadContext(modelAssemblyFile);
        var appAssembly = loadContext.LoadFromAssemblyPath(modelAssemblyFile);
        loadContext.LoadAllAssemblies();

        Assembly? coreAssembly = appAssembly.GetReferencedAssemblies()
            .Where(a => a.Name?.StartsWith("NForza.Cyrus.Core") ?? false)
            .Select(loadContext.LoadFromAssemblyName)
            .FirstOrDefault();

        if (coreAssembly is null)
        {
            Console.WriteLine("NForza.Cyrus.Core assembly not found in referenced assemblies.");
            return (false, null);
        }

        var cyrusModelType = coreAssembly.GetType(typeof(CyrusModel).FullName);
        if (cyrusModelType is null)
        {
            Console.WriteLine("CyrusModel type not found in core assembly.");
            return (false, null);
        }

        var getAsJsonMethod = cyrusModelType.GetMethod("GetAsJson", BindingFlags.Public | BindingFlags.Static);
        if (getAsJsonMethod is null)
        {
            Console.WriteLine("GetAsJson method not found in CyrusModel type.");
            return (false, null);
        }

        var json = getAsJsonMethod.Invoke(null, null);
        if (json is null)
        {
            Console.WriteLine("GetAsJson returned null.");
            return (false, null);
        }
        return (true, json.ToString());
    }
}
