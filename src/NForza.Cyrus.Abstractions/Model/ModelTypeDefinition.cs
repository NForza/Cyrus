namespace NForza.Cyrus.Abstractions.Model;
public record ModelTypeDefinition(string Name, IEnumerable<ModelPropertyDefinition> Properties, IEnumerable<string> Values) : INamedModelType
{
}
