using System.Collections.Generic;
using System;

namespace NForza.Cyrus.Abstractions.Model;

public class TypeWithPropertiesEqualityComparer : IEqualityComparer<ModelTypeDefinition>
{
    public static TypeWithPropertiesEqualityComparer Instance { get; } = new TypeWithPropertiesEqualityComparer();
    public bool Equals(ModelTypeDefinition x, ModelTypeDefinition y)
    {
        if (x == null || y == null) return false;
        return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(ModelTypeDefinition obj)
    {
        return obj.Name?.GetHashCode() ?? 0;
    }
}
