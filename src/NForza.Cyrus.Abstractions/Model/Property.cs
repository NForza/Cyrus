using System;

namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelProperty : ITypeDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsCollection { get; set; } = false;
        public bool IsNullable { get; set; } = false;
        public ModelTypeDefinition[] SupportTypes { get; set; } = Array.Empty<ModelTypeDefinition>();
    }                       
}