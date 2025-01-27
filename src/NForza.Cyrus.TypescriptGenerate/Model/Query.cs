namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Query : ITypeWithProperties
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
}