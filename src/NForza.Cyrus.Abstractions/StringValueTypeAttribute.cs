using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct)]
public class StringValueAttribute : Attribute
{
    public StringValueAttribute(int minimumLength, int maximumLength)
    {
        MinimumLength = minimumLength;
        MaximumLength = maximumLength;
    }

    public StringValueAttribute() : this(-1, -1)
    {
    }
    public StringValueAttribute(int minimumLength) : this(minimumLength, -1)
    {
    }

    public int MinimumLength { get; }
    public int MaximumLength { get; }
}
