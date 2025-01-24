namespace NForza.Cyrus.Abstractions.Model
{
    public record ModelQueryDefinition(string Name, string ReturnType, bool ReturnsCollection, bool ReturnsNullable);
}