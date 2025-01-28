namespace NForza.Cyrus.Abstractions.Model;

public record QueryReturnType(string Name, bool IsCollection, bool IsNullable, ModelPropertyDefinition[] Properties);
