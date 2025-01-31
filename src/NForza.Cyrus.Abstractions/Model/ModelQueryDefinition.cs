namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelQueryDefinition
    {
        public ModelQueryDefinition(string name, QueryReturnType returnType)
        {
            Name = name;
            ReturnType = returnType;
        }

        public string Name { get; }
        public QueryReturnType ReturnType { get; }
    }
}