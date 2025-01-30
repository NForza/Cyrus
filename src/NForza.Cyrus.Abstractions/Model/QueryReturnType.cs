namespace NForza.Cyrus.Abstractions.Model;

public record QueryReturnType(
    string Type, 
    bool IsCollection, 
    bool IsNullable,
    ModelPropertyDefinition[] Properties, 
    ModelTypeDefinition[] SupportTypes);
