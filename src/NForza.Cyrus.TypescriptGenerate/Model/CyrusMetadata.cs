namespace NForza.Cyrus.TypescriptGenerate.Model;

public class CyrusMetadata
{
    public Command[] Commands { get; set; } = [];
    public Event[] Events { get; set; } = [];
    public Query[] Queries { get; set; } = [];
    public string[] Guids { get; set; } = [];
    public string[] Strings { get; set; } = [];
    public Hub[] Hubs { get; set; } = [];
}
