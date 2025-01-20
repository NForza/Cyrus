namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Property
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsCollection { get; set; } = false;
    public bool IsNullable { get; set; } = false;
}