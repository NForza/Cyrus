namespace NForza.Cyrus.Abstractions.Model;

public class ModelQueryDefinition : ModelTypeDefinition
{
    public ModelQueryDefinition(ModelTypeDefinition queryTypeDefinition, ModelTypeDefinition returnType)
        : this(queryTypeDefinition.Name, queryTypeDefinition.ClrTypeName, queryTypeDefinition.Description, queryTypeDefinition.Properties, queryTypeDefinition.Values, queryTypeDefinition.IsCollection, queryTypeDefinition.IsNullable, returnType)
    {
    }

    public ModelQueryDefinition(string name, string clrTypeName, string description, ModelPropertyDefinition[] properties, string[] values, bool isCollection, bool isNullable, ModelTypeDefinition returnType)
        : base(name, clrTypeName, description, properties, values, isCollection, isNullable)
    {
        ReturnType = returnType;
    }
    public ModelTypeDefinition ReturnType { get; set; }
}