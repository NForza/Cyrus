using System.Linq;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Hub 
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string[] Events { get; set; } = [];
    public HubQuery[] Queries { get; set; } = [];
    public string[] Commands { get; set; } = [];
    public string[] Imports => Events.Concat(Commands).Concat(Queries.Select(hq => hq.Name)).Concat(Queries.Select(hq => hq.ReturnType.Name).Where(name => !name.IsBuiltInTypeScriptType())).ToArray();
}