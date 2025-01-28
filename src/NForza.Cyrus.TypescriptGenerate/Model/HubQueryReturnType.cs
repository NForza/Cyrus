namespace NForza.Cyrus.TypescriptGenerate.Model;

public class HubQueryReturnType : ITypeWithProperties
{
    public string Name { get; set; } = string.Empty;
    public bool IsCollection { get; set; }
    public bool IsNullable { get; set; }
    public Property[] Properties { get; set ; } = [];
}