namespace NForza.Cyrus.Abstractions.Model
{
    public record ModelPropertyDefinition(string Name, string Type, bool IsCollection, bool IsNullable): INamedModelType;
}