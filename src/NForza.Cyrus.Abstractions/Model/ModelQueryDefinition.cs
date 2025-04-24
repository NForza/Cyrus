namespace NForza.Cyrus.Abstractions.Model;

public class ModelQueryDefinition
{
    public ModelQueryDefinition(string name, ModelTypeDefinition returnType)
    {
        Name = name;
        ReturnType = returnType;
    }

    public string Name { get; }
    public ModelTypeDefinition ReturnType { get; }
}