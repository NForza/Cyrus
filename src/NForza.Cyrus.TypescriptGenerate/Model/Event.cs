namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Event : ITypeWithProperties
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
}