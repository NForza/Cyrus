using System;

namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelTypeDefinition
    {
        public ModelTypeDefinition(string name, ModelPropertyDefinition[] properties, bool isCollection, bool isNullable) //, ModelTypeDefinition[] supportTypes)
        {
            Name = name;
            Properties = properties;
            IsCollection = isCollection;
            IsNullable = isNullable;
//            SupportTypes = supportTypes;
        }
        public string Name { get; }= string.Empty;
        public bool IsCollection { get; } = false;
        public bool IsNullable { get; } = false;

        public ModelPropertyDefinition[] Properties { get; } = Array.Empty<ModelPropertyDefinition>();
//        public ModelTypeDefinition[] SupportTypes { get;  } = Array.Empty<ModelTypeDefinition>();
    }
}
