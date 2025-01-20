namespace NForza.Cyrus.TypescriptGenerate.Model;


public class Command: IMetaDataWithProperties
{
    public string Name { get; set; } = string.Empty;
    public Property[] Properties { get; set; } = [];
}

