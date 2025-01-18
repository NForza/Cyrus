namespace NForza.Cyrus.Abstractions.Model;

public class ModelDefinitionEqualityComparer : IEqualityComparer<ModelDefinition>
{
    public static ModelDefinitionEqualityComparer Instance { get; } = new ModelDefinitionEqualityComparer();

    public bool Equals(ModelDefinition? x, ModelDefinition? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return x.Name == y.Name && x.Properties.SequenceEqual(y.Properties);
    }

    public int GetHashCode(ModelDefinition obj)
    {
        return HashCode.Combine(obj.Name, obj.Properties);
    }
}
public record ModelDefinition(string Name, IEnumerable<PropertyModelDefinition> Properties)
{
}
