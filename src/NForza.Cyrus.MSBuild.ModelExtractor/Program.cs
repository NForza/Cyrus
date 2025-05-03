using System.Reflection;

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

var cyrusModel = coreAssembly.GetType("NForza.Cyrus.Model.CyrusModel");
if (cyrusModel is null)
{
    Console.WriteLine("CyrusModel type not found in core assembly.");
    return 1;
}

var getAsJsonMethod = cyrusModel.GetMethod("GetAsJson", BindingFlags.Public | BindingFlags.Static);
if (getAsJsonMethod is null)
{
    Console.WriteLine("GetModelAsJson method not found in CyrusModel type.");
    return 1;
}

var json = getAsJsonMethod.Invoke(null, null);
if (json is null)
{
    Console.WriteLine("GetModelAsJson returned null.");
    return 1;
}

Console.WriteLine(json);

return 0;
