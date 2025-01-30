namespace NForza.Cyrus.TypescriptGenerate.Model;


public class Command: ITypeWithProperties
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
    public SupportType[] SupportTypes { get; set; } = [];
}

