using System;

namespace NForza.Cyrus.Abstractions
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Route { get; set; }
        public HttpVerb Verb { get; set; }
    }
}
