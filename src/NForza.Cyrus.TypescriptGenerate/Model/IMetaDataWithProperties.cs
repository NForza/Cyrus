namespace NForza.Cyrus.TypescriptGenerate.Model;

public interface ITypeWithProperties
{
    public string Name { get; set; }
    Property[] Properties { get; set; }
    SupportType[] SupportTypes { get; set; }
}