namespace NForza.TypedIds;

[AttributeUsage(AttributeTargets.Struct)]
public class StringId(int minimumLength, int maximumLength) : Attribute
{
    public int MinimumLength { get; } = minimumLength;
    public int MaximumLength { get; } = maximumLength;
}
