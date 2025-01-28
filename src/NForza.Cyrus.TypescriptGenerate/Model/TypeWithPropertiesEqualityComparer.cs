using System.Collections.Generic;
using System;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public class TypeWithPropertiesEqualityComparer : IEqualityComparer<ITypeWithProperties>
{
    public static TypeWithPropertiesEqualityComparer Instance { get; } = new TypeWithPropertiesEqualityComparer();
    public bool Equals(ITypeWithProperties? x, ITypeWithProperties? y)
    {
        if (x == null || y == null) return false;
        return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(ITypeWithProperties obj)
    {
        return obj.Name?.GetHashCode(StringComparison.InvariantCultureIgnoreCase) ?? 0;
    }
}
