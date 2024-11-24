namespace NForza.Cyrus.TypedIds;

public class TypedIdDictionary(Dictionary<Type, Type> values) : Dictionary<Type, Type>(values)
{
}
