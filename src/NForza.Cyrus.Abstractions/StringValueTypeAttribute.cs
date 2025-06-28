using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct)]
public class StringValueAttribute : Attribute
{
    public StringValueAttribute(int minimumLength, int maximumLength, string validationRegex): this(minimumLength, maximumLength)
    {
        ValidationRegex = validationRegex;
    }

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
    public string ValidationRegex { get; } = string.Empty;
}
