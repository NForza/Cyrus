using System.Collections.Generic;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public class NamedTypeEqualityComparer : IEqualityComparer<ModelTypeDefinition>
{
    public static NamedTypeEqualityComparer Instance => new NamedTypeEqualityComparer();

    public bool Equals(ModelTypeDefinition? x, ModelTypeDefinition? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return x.Name == y.Name;
    }

    public int GetHashCode(ModelTypeDefinition obj)
    {
        return obj.Name.GetHashCode();
    }
}
