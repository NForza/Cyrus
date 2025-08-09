using System;

namespace NForza.Cyrus.Abstractions.Model;

public class ModelQueryDefinition : ModelTypeDefinition
{
    public ModelQueryDefinition(ModelTypeDefinition queryTypeDefinition, ModelTypeDefinition returnType)
        : this(queryTypeDefinition.Name, queryTypeDefinition.ClrTypeName, queryTypeDefinition.Properties, queryTypeDefinition.Values, queryTypeDefinition.IsCollection, queryTypeDefinition.IsNullable, returnType)
    {
    }

    public ModelQueryDefinition(string name, string clrTypeName, ModelPropertyDefinition[] properties, string[] values, bool isCollection, bool isNullable, ModelTypeDefinition returnType)
        : base(name, clrTypeName, properties, values, isCollection, isNullable)
    {
        ReturnType = returnType;
    }
    public ModelTypeDefinition ReturnType { get; set; }
}