namespace NForza.Cyrus.Abstractions.Model;

public class ModelQueryDefinition : ModelTypeDefinition
{
    public ModelQueryDefinition(ModelTypeDefinition queryTypeDefinition, ModelTypeDefinition returnType) 
        : this(queryTypeDefinition.Name, queryTypeDefinition.Properties, queryTypeDefinition.Values, queryTypeDefinition.IsCollection, queryTypeDefinition.IsNullable, returnType)
    {
    }

    public ModelQueryDefinition() : base("", [], [], false, false)
    {
        ReturnType = default!;
    }

    public ModelQueryDefinition(string name, ModelPropertyDefinition[] properties, string[] values, bool isCollection, bool isNullable, ModelTypeDefinition returnType)
        : base(name, properties, values, isCollection, isNullable)
    {
        ReturnType = returnType;
    }
    public ModelTypeDefinition ReturnType { get; set; }
}