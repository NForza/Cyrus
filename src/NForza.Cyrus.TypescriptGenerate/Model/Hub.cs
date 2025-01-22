namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Hub 
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string[] Events { get; set; } = [];
    public string[] Commands { get; set; } = [];
    public string[] Imports { get; set; } = [];
}