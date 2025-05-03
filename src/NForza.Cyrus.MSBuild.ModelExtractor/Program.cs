using System.Reflection;
using NForza.Cyrus.Model;

var modelAssemblyFile = args.Length > 0 ? args[0] : "";
if (!File.Exists(modelAssemblyFile))
{
    Console.WriteLine($"Model assembly file not found: {modelAssemblyFile}");
    return 1;
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
    return 1;
}

var cyrusModelType = coreAssembly.GetType(typeof(CyrusModel).FullName);
if (cyrusModelType is null)
{
    Console.WriteLine("CyrusModel type not found in core assembly.");
    return 1;
}

var getAsJsonMethod = cyrusModelType.GetMethod("GetAsJson", BindingFlags.Public | BindingFlags.Static);
if (getAsJsonMethod is null)
{
    cyrusModelType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .ToList()
        .ForEach(m => Console.WriteLine(m.Name));
    Console.WriteLine("");
    cyrusModelType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .ToList()
        .ForEach(m => Console.WriteLine(m.Name));
    Console.WriteLine("GetAsJson method not found in CyrusModel type.");
    return 1;
}

var json = getAsJsonMethod.Invoke(null, null);
if (json is null)
{
    Console.WriteLine("GetAsJson returned null.");
    return 1;
}

Console.WriteLine(json);

return 0;
