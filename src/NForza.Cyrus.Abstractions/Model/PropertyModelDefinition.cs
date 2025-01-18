namespace NForza.Cyrus.Abstractions.Model
{
    public record PropertyModelDefinition(string Name, string Type, bool isCollection, bool isNullable);
}