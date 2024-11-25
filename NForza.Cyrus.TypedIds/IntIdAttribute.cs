namespace NForza.Cyrus.TypedIds;

[AttributeUsage(AttributeTargets.Struct)]
public class IntIdAttribute(int minimum, int maximum) : Attribute
{
    public IntIdAttribute() : this(int.MinValue, int.MaxValue)
    {
    }
    public IntIdAttribute(int minimum) : this(minimum, int.MaxValue)
    {
    }

    public int Minimum { get; } = minimum;
    public int Maximum { get; } = maximum;
}
