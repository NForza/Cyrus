using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct)]
public class IntIdAttribute : Attribute
{
    public IntIdAttribute(int minimum, int maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public IntIdAttribute() : this(int.MinValue, int.MaxValue)
    {
    }
    public IntIdAttribute(int minimum) : this(minimum, int.MaxValue)
    {
    }

    public int Minimum { get; }
    public int Maximum { get; }
}
