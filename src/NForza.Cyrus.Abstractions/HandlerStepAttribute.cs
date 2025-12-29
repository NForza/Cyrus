using System;

namespace NForza.Cyrus.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HandlerStepAttribute(string methodName) : Attribute
    {
    }
}