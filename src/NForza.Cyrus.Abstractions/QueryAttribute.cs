using System;

namespace NForza.Cyrus.Abstractions
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class QueryAttribute: Attribute
    {
        public string? Route { get; set; } = null;
    }
}
