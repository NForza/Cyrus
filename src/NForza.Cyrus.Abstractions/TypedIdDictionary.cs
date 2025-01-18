namespace NForza.Cyrus.Abstractions;

public class TypedIdDictionary(Dictionary<Type, Type> values) : Dictionary<Type, Type>(values)
{
}
