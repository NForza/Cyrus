using System;

namespace NForza.Cyrus.Abstractions
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class StringIdAttribute : Attribute
    {
        public StringIdAttribute(int minimumLength, int maximumLength)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }

        public StringIdAttribute() : this(-1, -1)
        {
        }
        public StringIdAttribute(int minimumLength) : this(minimumLength, -1)
        {
        }

        public int MinimumLength { get; }
        public int MaximumLength { get; }
    }
}
