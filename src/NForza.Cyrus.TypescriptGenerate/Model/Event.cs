namespace NForza.Cyrus.TypescriptGenerate.Model;

public class Event : IMetaDataWithProperties
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
}