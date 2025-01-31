namespace NForza.Cyrus.TypescriptGenerate.Model;

public class SupportType : ITypeWithProperties, ITypeWithValues
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
    public string[] Values { get; set; } = [];
    public SupportType[] SupportTypes { get; set; } = [];
}
