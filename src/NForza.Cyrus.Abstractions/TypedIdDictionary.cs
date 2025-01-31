using System;
using System.Collections.Generic;

namespace NForza.Cyrus.Abstractions
{
    public class TypedIdDictionary : Dictionary<Type, Type>
    {
        public TypedIdDictionary(Dictionary<Type, Type> values) : base(values)
        {
        }
    }
}
