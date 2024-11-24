namespace NForza.Cyrus.TypedIds;

[AttributeUsage(AttributeTargets.Struct)]
public class StringIdAttribute(int minimumLength, int maximumLength) : Attribute
{
    public StringIdAttribute() : this(-1, -1)
    {
    }
    public StringIdAttribute(int minimumLength) : this(minimumLength, -1)
    {
    }

    public int MinimumLength { get; } = minimumLength;
    public int MaximumLength { get; } = maximumLength;
}
