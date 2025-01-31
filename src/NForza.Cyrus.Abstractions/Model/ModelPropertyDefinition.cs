namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelPropertyDefinition : INamedModelType
    {
        public ModelPropertyDefinition(string name, string type, bool isCollection, bool isNullable)
        {
            Name = name;
            Type = type;
            IsCollection = isCollection;
            IsNullable = isNullable;
        }
        public string Name { get; }
        public string Type { get; }
        public bool IsCollection { get; }
        public bool IsNullable { get; }
    }
}
