namespace NForza.Cyrus.TypescriptGenerate.Model;

public class HubQuery
{
    public string Name { get; set; } = string.Empty;
    public HubQueryReturnType ReturnType { get; set; } = new();
}