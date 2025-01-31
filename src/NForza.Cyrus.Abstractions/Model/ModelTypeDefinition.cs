namespace NForza.Cyrus.Abstractions.Model
{
    public class ModelTypeDefinition : INamedModelType
    {
        public ModelTypeDefinition(string name, IEnumerable<ModelPropertyDefinition> properties)
        {
            Name = name;
            Properties = properties;
        }
        public string Name { get; }
        public IEnumerable<ModelPropertyDefinition> Properties { get; }
    }
}
