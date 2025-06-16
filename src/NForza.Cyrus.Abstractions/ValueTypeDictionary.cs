using System;
using System.Collections.Generic;

namespace NForza.Cyrus.Abstractions;

public class ValueTypeDictionary : Dictionary<Type, Type>
{
    public ValueTypeDictionary(Dictionary<Type, Type> values) : base(values)
    {
    }
}
