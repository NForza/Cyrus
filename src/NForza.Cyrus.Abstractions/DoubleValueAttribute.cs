using System;

namespace NForza.Cyrus.Abstractions;

[AttributeUsage(AttributeTargets.Struct)]
public class DoubleValueAttribute : Attribute
{
    public DoubleValueAttribute(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public DoubleValueAttribute() : this(double.NaN, double.NaN)
    {
    }
    public DoubleValueAttribute(double minimum) : this(minimum, double.NaN)
    {
    }

    public double Minimum { get; }
    public double Maximum { get; }
}
