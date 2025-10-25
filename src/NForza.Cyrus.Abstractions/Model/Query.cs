namespace NForza.Cyrus.Abstractions.Model;

public class Query
{
    public string Name { get; set; } = string.Empty;
    public ModelTypeDefinition ReturnType { get; set; } = new ModelTypeDefinition(string.Empty, string.Empty, string.Empty,
        [], [], false, false); 
}