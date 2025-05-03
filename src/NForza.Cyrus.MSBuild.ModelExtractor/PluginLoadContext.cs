using System.Reflection;
using System.Runtime.Loader;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly string _pluginDirectory;

    public PluginLoadContext(string pluginPath)
    {
        _pluginDirectory = Path.GetDirectoryName(pluginPath)!;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name.StartsWith("System.") ||
            assemblyName.Name == "netstandard" ||
            assemblyName.Name.StartsWith("Microsoft."))
        {
            return null; 
        }

        string depPath = Path.Combine(_pluginDirectory, $"{assemblyName.Name}.dll");

        if (File.Exists(depPath))
        {
            return LoadFromAssemblyPath(depPath);
        }

        return null;
    }

    public void LoadAllAssemblies()
    {
        foreach (var dll in Directory.GetFiles(_pluginDirectory, "*.dll"))
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(dll);

                // Skip if already loaded (by name)
                if (Assemblies.Any(a => a.GetName().Name == name.Name))
                    continue;

                LoadFromAssemblyPath(dll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginLoadContext] Skipping {dll}: {ex.Message}");
            }
        }
    }
}
