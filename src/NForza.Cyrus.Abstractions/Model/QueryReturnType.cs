namespace NForza.Cyrus.Abstractions.Model
{
    public class QueryReturnType
    {
        public QueryReturnType(string type, bool isCollection, bool isNullable, ModelPropertyDefinition[] properties)
        {
            Type = type;
            IsCollection = isCollection;
            IsNullable = isNullable;
            Properties = properties;
        }

        public string Type { get; }
        public bool IsCollection { get; }
        public bool IsNullable { get; }
        public ModelPropertyDefinition[] Properties { get; }
    }
}
