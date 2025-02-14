using System;

namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelTypeDefinition
    {
        public ModelTypeDefinition(string name, ModelPropertyDefinition[] properties, string[] values, bool isCollection, bool isNullable)
        {
            Name = name;
            Properties = properties;
            IsCollection = isCollection;
            IsNullable = isNullable;
            Values = values;
        }
        public string Name { get; }= string.Empty;
        public bool IsCollection { get; } = false;
        public bool IsNullable { get; } = false;

        public ModelPropertyDefinition[] Properties { get; } = Array.Empty<ModelPropertyDefinition>();
        public string[] Values { get; } = Array.Empty<string>();
    }
}
