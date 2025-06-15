using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct)]
public class IntValueAttribute : Attribute
{
    public IntValueAttribute(int minimum, int maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public IntValueAttribute() : this(int.MinValue, int.MaxValue)
    {
    }
    public IntValueAttribute(int minimum) : this(minimum, int.MaxValue)
    {
    }

    public int Minimum { get; }
    public int Maximum { get; }
}
